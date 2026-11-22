using AutoMapper;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Repositories;

namespace MyWebAPI.Services;

public class StorageTenantService : IStorageTenantService
{
    private readonly IStorageTenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public StorageTenantService(IStorageTenantRepository tenantRepository, IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StorageTenantResponseDto>> GetAllAsync(int? count = null)
    {
        if (count.HasValue && count.Value <= 0)
            throw new ArgumentException("Кількість записів повинна бути більшою за 0.");

        var tenants = await _tenantRepository.GetAllAsync();
        var result = count.HasValue ? tenants.Take(count.Value).ToList() : tenants;
        return _mapper.Map<IReadOnlyList<StorageTenantResponseDto>>(result);
    }

    public async Task<StorageTenantResponseDto> GetByIdAsync(string id)
    {
        var tenant = await GetEntityByIdAsync(id);
        return _mapper.Map<StorageTenantResponseDto>(tenant);
    }

    public async Task<StorageTenantResponseDto> CreateAsync(StorageTenantCreateDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Неможливо створити орендаря з порожніми даними.");

        var created = await _tenantRepository.CreateAsync(dto);
        return _mapper.Map<StorageTenantResponseDto>(created);
    }

    public async Task<StorageTenantResponseDto> FullUpdateAsync(string id, StorageTenantCreateDto dto)
    {
        var tenant = await GetEntityByIdAsync(id);

        tenant.FullName = dto.FullName;
        tenant.PhoneNumber = dto.PhoneNumber;
        tenant.Email = dto.Email;
        tenant.Type = dto.Type;

        await _tenantRepository.ReplaceAsync(id, tenant);
        return _mapper.Map<StorageTenantResponseDto>(tenant);
    }

    public async Task<StorageTenantResponseDto> PartialUpdateAsync(string id, StorageTenantPatchDto dto)
    {
        var updated = await _tenantRepository.UpdateAsync(id, dto);
        if (updated == null)
            throw new KeyNotFoundException($"Орендаря з ID {id} не знайдено.");

        return _mapper.Map<StorageTenantResponseDto>(updated);
    }

    public async Task<StorageTenantResponseDto> PayDebtAsync(string id)
    {
        var tenant = await GetEntityByIdAsync(id);

        if (!tenant.HasDebt)
            throw new InvalidOperationException("В орендаря немає активної заборгованості.");

        tenant.HasDebt = false;
        await _tenantRepository.ReplaceAsync(id, tenant);
        return _mapper.Map<StorageTenantResponseDto>(tenant);
    }

    public async Task DeleteAsync(string id)
    {
        var tenant = await GetEntityByIdAsync(id);

        if (tenant.HasDebt)
            throw new InvalidOperationException("Неможливо видалити орендаря з наявною заборгованістю.");

        if (tenant.ContractsId.Any())
            throw new InvalidOperationException("Неможливо видалити орендаря, за яким закріплені договори оренди.");

        await _tenantRepository.DeleteAsync(id);
    }

    private async Task<StorageTenant> GetEntityByIdAsync(string id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            throw new KeyNotFoundException($"Орендаря з ID {id} не знайдено.");

        return tenant;
    }
}