using AutoMapper;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Repositories;

namespace MyWebAPI.Services;

public class StorageLeaseContractService : IStorageLeaseContractService
{
    private readonly IStorageLeaseContractRepository _contractRepository;
    private readonly IStorageBoxRepository _boxRepository;
    private readonly IStorageTenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public StorageLeaseContractService(
        IStorageLeaseContractRepository contractRepository,
        IStorageBoxRepository boxRepository,
        IStorageTenantRepository tenantRepository,
        IMapper mapper)
    {
        _contractRepository = contractRepository;
        _boxRepository = boxRepository;
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StorageLeaseContractResponseDto>> GetAllAsync(int? count = null)
    {
        if (count.HasValue && count.Value <= 0)
            throw new ArgumentException("Кількість записів повинна бути більшою за 0.");

        var contracts = await _contractRepository.GetAllAsync();
        var result = count.HasValue ? contracts.Take(count.Value).ToList() : contracts;
        return _mapper.Map<IReadOnlyList<StorageLeaseContractResponseDto>>(result);
    }

    public async Task<StorageLeaseContractResponseDto> GetByIdAsync(string id)
    {
        var contract = await GetEntityByIdAsync(id);
        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task<StorageLeaseContractResponseDto> CreateContractAsync(StorageLeaseContractCreateDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Дані договору порожні.");

        var box = await _boxRepository.GetByIdAsync(dto.StorageBoxId);
        if (box == null)
            throw new KeyNotFoundException($"Бокс із ID {dto.StorageBoxId} не знайдено.");

        if (box.Status != StorageBoxStatus.Available || !box.IsDisinfected)
            throw new InvalidOperationException("Бокс повинен перебувати у статусі Available та бути продезінфікованим після попереднього орендаря.");

        var tenant = await _tenantRepository.GetByIdAsync(dto.StorageTenantId);
        if (tenant == null)
            throw new KeyNotFoundException($"Орендаря з ID {dto.StorageTenantId} не знайдено.");

        if (tenant.HasDebt)
            throw new InvalidOperationException("Орендар має заборгованість і не може бронювати бокс.");

        decimal monthlyRate = dto.DurationDays >= 180 ? box.MonthlyPrice * 0.9m : box.MonthlyPrice;

        var newContract = new StorageLeaseContract
        {
            StorageBoxId = dto.StorageBoxId,
            StorageTenantId = dto.StorageTenantId,
            DurationDays = dto.DurationDays,
            StartDate = DateTime.UtcNow,
            Status = ContractStatus.PendingDeposit,
            MonthlyRate = monthlyRate,
            DepositAmount = monthlyRate,
            IsDepositPaid = false,
            AccessPinCode = null,
            IsPinActive = false
        };

        var created = await _contractRepository.CreateAsync(newContract);

        box.Status = StorageBoxStatus.Reserved;
        box.CurrentContractId = created.Id;
        await _boxRepository.ReplaceAsync(box.Id!, box);

        await _tenantRepository.AddContractAsync(tenant.Id!, created.Id!);

        return _mapper.Map<StorageLeaseContractResponseDto>(created);
    }

    public async Task<StorageLeaseContractResponseDto> PayDepositAsync(string id, decimal value)
    {
        var contract = await GetEntityByIdAsync(id);

        if (contract.Status != ContractStatus.PendingDeposit)
            throw new InvalidOperationException("Оплатити депозит можна лише для договору в статусі PendingDeposit.");

        if (DateTime.UtcNow > contract.ReservedUntil)
            throw new InvalidOperationException("Час бронювання боксу (24 години) вичерпано.");

        decimal totalRequired = contract.MonthlyRate + contract.DepositAmount;
        if (value < totalRequired)
            throw new InvalidOperationException($"Недостатньо коштів. Потрібно сплатити {totalRequired} грн.");

        contract.IsDepositPaid = true;
        contract.Status = ContractStatus.Active;
        contract.AccessPinCode = Random.Shared.Next(100000, 999999).ToString();
        contract.IsPinActive = true;

        await _contractRepository.ReplaceAsync(id, contract);

        var box = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
        if (box != null)
        {
            box.Status = StorageBoxStatus.Occupied;
            box.CurrentContractId = contract.Id;
            await _boxRepository.ReplaceAsync(box.Id!, box);
        }

        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task<string> GetPaymentNotificationAsync(string id)
    {
        var contract = await GetEntityByIdAsync(id);
        int daysRemaining = (contract.NextPaymentDueDate - DateTime.UtcNow).Days;

        if (contract.IsInvoice())
        {
            if (daysRemaining < 0)
                return $"Оплата прострочена вже {-daysRemaining} днів.";

            return $"Нагадування про сплату. До наступного платежу залишилося днів: {daysRemaining}. Будь ласка, сплатіть вчасно, інакше доступ до боксу буде заблоковано.";
        }

        return $"Договір активний. До наступного платежу залишилося днів: {daysRemaining}.";
    }

    public async Task<StorageLeaseContractResponseDto> CheckOverdueAndSuspendAsync(string id)
    {
        var contract = await GetEntityByIdAsync(id);

        if (!contract.IsExpired())
            throw new InvalidOperationException($"Прострочення дедлайну ({contract.NextPaymentDueDate:yyyy-MM-dd}) ще не настало.");

        contract.Status = ContractStatus.Suspended;
        contract.IsPinActive = false;
        await _contractRepository.ReplaceAsync(id, contract);

        await _tenantRepository.SetDebtAsync(contract.StorageTenantId, true);

        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task<StorageLeaseContractResponseDto> CloseContractAsync(string id)
    {
        var contract = await GetEntityByIdAsync(id);

        contract.Status = ContractStatus.Closed;
        contract.IsPinActive = false;
        await _contractRepository.ReplaceAsync(id, contract);

        var box = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
        if (box != null)
        {
            box.Status = StorageBoxStatus.Available;
            box.IsDisinfected = false;
            box.CurrentContractId = null;
            await _boxRepository.ReplaceAsync(box.Id!, box);
        }

        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task<StorageLeaseContractResponseDto> UpdateContractAsync(string id, StorageLeaseContractCreateDto dto)
    {
        var contract = await GetEntityByIdAsync(id);
        var newBox = await _boxRepository.GetByIdAsync(dto.StorageBoxId);
        if (newBox == null)
            throw new KeyNotFoundException($"Бокс із ID {dto.StorageBoxId} не знайдено.");

        if (contract.StorageBoxId != dto.StorageBoxId)
        {
            var oldBox = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
            if (oldBox != null)
            {
                oldBox.CurrentContractId = null;
                await _boxRepository.ReplaceAsync(oldBox.Id!, oldBox);
            }

            newBox.CurrentContractId = id;
            await _boxRepository.ReplaceAsync(newBox.Id!, newBox);
        }

        contract.StorageBoxId = dto.StorageBoxId;
        contract.StorageTenantId = dto.StorageTenantId;
        contract.DurationDays = dto.DurationDays;
        contract.MonthlyRate = dto.DurationDays >= 180 ? newBox.MonthlyPrice * 0.9m : newBox.MonthlyPrice;
        contract.DepositAmount = contract.MonthlyRate;

        await _contractRepository.ReplaceAsync(id, contract);
        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task<StorageLeaseContractResponseDto> PatchContractAsync(string id, StorageLeaseContractPatchDto dto)
    {
        var contract = await GetEntityByIdAsync(id);

        if (!string.IsNullOrWhiteSpace(dto.StorageTenantId))
        {
            var tenant = await _tenantRepository.GetByIdAsync(dto.StorageTenantId);
            if (tenant == null)
                throw new KeyNotFoundException($"Орендаря з ID {dto.StorageTenantId} не знайдено.");

            contract.StorageTenantId = dto.StorageTenantId;
        }

        if (!string.IsNullOrWhiteSpace(dto.StorageBoxId) && dto.StorageBoxId != contract.StorageBoxId)
        {
            var newBox = await _boxRepository.GetByIdAsync(dto.StorageBoxId);
            if (newBox == null)
                throw new KeyNotFoundException($"Бокс із ID {dto.StorageBoxId} не знайдено.");

            var oldBox = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
            if (oldBox != null)
            {
                oldBox.CurrentContractId = null;
                await _boxRepository.ReplaceAsync(oldBox.Id!, oldBox);
            }

            newBox.CurrentContractId = id;
            await _boxRepository.ReplaceAsync(newBox.Id!, newBox);

            contract.StorageBoxId = dto.StorageBoxId;
        }

        if (dto.DurationDays.HasValue)
        {
            contract.DurationDays = dto.DurationDays.Value;
            var currentBox = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
            if (currentBox != null)
            {
                contract.MonthlyRate = contract.DurationDays >= 180 ? currentBox.MonthlyPrice * 0.9m : currentBox.MonthlyPrice;
                contract.DepositAmount = contract.MonthlyRate;
            }
        }

        await _contractRepository.ReplaceAsync(id, contract);
        return _mapper.Map<StorageLeaseContractResponseDto>(contract);
    }

    public async Task DeleteContractAsync(string id)
    {
        var contract = await GetEntityByIdAsync(id);

        await _tenantRepository.RemoveContractAsync(contract.StorageTenantId, contract.Id!);

        var box = await _boxRepository.GetByIdAsync(contract.StorageBoxId);
        if (box != null && box.CurrentContractId == id)
        {
            box.Status = StorageBoxStatus.Available;
            box.IsDisinfected = false;
            box.CurrentContractId = null;
            await _boxRepository.ReplaceAsync(box.Id!, box);
        }

        await _contractRepository.DeleteAsync(id);
    }

    private async Task<StorageLeaseContract> GetEntityByIdAsync(string id)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null)
            throw new KeyNotFoundException($"Договір із ID {id} не знайдено.");

        return contract;
    }
}