using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public interface IStorageBoxRepository
{
    Task<IReadOnlyList<StorageBox>> GetAllAsync();
    Task<StorageBox?> GetByIdAsync(string id);
    Task<IReadOnlyList<StorageBox>> GetAvailableAsync(StorageBoxType? type = null, double? minVolume = null);
    Task<StorageBox> CreateAsync(StorageBoxCreateDto dto);
    Task<bool> ReplaceAsync(string id, StorageBox box);
    Task<StorageBox?> UpdateAsync(string id, StorageBoxPatchDto dto);
    Task<bool> UpdateStatusAndDisinfectionAsync(string id, StorageBoxStatus? status, bool? isDisinfected);
    Task<bool> DeleteAsync(string id);
}