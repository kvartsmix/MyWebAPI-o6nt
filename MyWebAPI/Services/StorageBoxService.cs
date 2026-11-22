using AutoMapper;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Repositories;

namespace MyWebAPI.Services;

public class StorageBoxService : IStorageBoxService
{
    private readonly IStorageBoxRepository _boxRepository;
    private readonly IMapper _mapper;

    public StorageBoxService(IStorageBoxRepository boxRepository, IMapper mapper)
    {
        _boxRepository = boxRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StorageBoxResponseDto>> GetAllAsync(int? count = null)
    {
        if (count.HasValue && count.Value <= 0)
            throw new ArgumentException("Кількість записів повинна бути більшою за 0.");

        var boxes = await _boxRepository.GetAllAsync();
        var result = count.HasValue ? boxes.Take(count.Value).ToList() : boxes;
        return _mapper.Map<IReadOnlyList<StorageBoxResponseDto>>(result);
    }

    public async Task<StorageBoxResponseDto> GetByIdAsync(string id)
    {
        var box = await GetEntityByIdAsync(id);
        return _mapper.Map<StorageBoxResponseDto>(box);
    }

    public async Task<IReadOnlyList<StorageBoxResponseDto>> GetAvailableAsync(StorageBoxType? type = null, double? minVolume = null)
    {
        var boxes = await _boxRepository.GetAvailableAsync(type, minVolume);
        return _mapper.Map<IReadOnlyList<StorageBoxResponseDto>>(boxes);
    }

    public async Task<StorageBoxResponseDto> CreateAsync(StorageBoxCreateDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Дані для створення боксу порожні.");

        var created = await _boxRepository.CreateAsync(dto);
        return _mapper.Map<StorageBoxResponseDto>(created);
    }

    public async Task<StorageBoxResponseDto> FullUpdateAsync(string id, StorageBoxCreateDto dto)
    {
        var box = await GetEntityByIdAsync(id);

        box.Square = dto.Square;
        box.Volume = dto.Volume;
        box.Type = dto.Type;
        box.IsDisinfected = dto.IsDisinfected;
        box.MonthlyPrice = dto.MonthlyPrice;

        await _boxRepository.ReplaceAsync(id, box);
        return _mapper.Map<StorageBoxResponseDto>(box);
    }

    public async Task<StorageBoxResponseDto> PartialUpdateAsync(string id, StorageBoxPatchDto dto)
    {
        var updated = await _boxRepository.UpdateAsync(id, dto);
        if (updated == null)
            throw new KeyNotFoundException($"Бокс із ID {id} не знайдено.");

        return _mapper.Map<StorageBoxResponseDto>(updated);
    }

    public async Task<StorageBoxResponseDto> DisinfectAsync(string id)
    {
        var box = await GetEntityByIdAsync(id);

        if (box.Status == StorageBoxStatus.Occupied)
            throw new InvalidOperationException("Неможливо дезінфікувати зайнятий бокс.");

        if (box.IsDisinfected)
            throw new InvalidOperationException("Бокс уже продезінфікований.");

        box.IsDisinfected = true;
        await _boxRepository.ReplaceAsync(id, box);
        return _mapper.Map<StorageBoxResponseDto>(box);
    }

    public async Task DeleteAsync(string id)
    {
        var box = await GetEntityByIdAsync(id);

        if (box.Status != StorageBoxStatus.Available || !string.IsNullOrEmpty(box.CurrentContractId))
            throw new InvalidOperationException("Неможливо видалити бокс, який наразі орендується або зарезервований.");

        await _boxRepository.DeleteAsync(id);
    }

    private async Task<StorageBox> GetEntityByIdAsync(string id)
    {
        var box = await _boxRepository.GetByIdAsync(id);
        if (box == null)
            throw new KeyNotFoundException($"Бокс із ID {id} не знайдено.");

        return box;
    }
    public async Task<decimal> CalculateOccupiedMonthlyRevenueAsync()
    {
        var boxes = await _boxRepository.GetAllAsync();

        return boxes
            .Where(b => b.Status == StorageBoxStatus.Occupied)
            .Sum(b => b.MonthlyPrice);
    }

    public async Task<StorageBoxResponseDto> GetMostExpensiveAvailableBoxAsync(double? maxVolume = null)
    {
        if (maxVolume.HasValue && maxVolume.Value <= 0)
            throw new ArgumentException("Максимальний об'єм повинен бути більшим за 0.", nameof(maxVolume));

        var boxes = await _boxRepository.GetAllAsync();

        var query = boxes.Where(b => b.Status == StorageBoxStatus.Available);

        if (maxVolume.HasValue)
            query = query.Where(b => b.Volume <= maxVolume.Value);

        var mostExpensive = query
            .OrderByDescending(b => b.MonthlyPrice)
            .FirstOrDefault();

        if (mostExpensive == null)
            throw new InvalidOperationException("Вільних боксів за вказаними критеріями не знайдено.");

        return _mapper.Map<StorageBoxResponseDto>(mostExpensive);
    }

    public async Task<int> CountReadyForRentBoxesAsync()
    {
        var boxes = await _boxRepository.GetAllAsync();

        return boxes.Count(b => b.Status == StorageBoxStatus.Available && b.IsDisinfected);
    }
}