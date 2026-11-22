using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Services;

public interface IStorageBoxService
{
    Task<IReadOnlyList<StorageBoxResponseDto>> GetAllAsync(int? count = null);
    Task<StorageBoxResponseDto> GetByIdAsync(string id);
    Task<IReadOnlyList<StorageBoxResponseDto>> GetAvailableAsync(StorageBoxType? type = null, double? minVolume = null);
    Task<StorageBoxResponseDto> CreateAsync(StorageBoxCreateDto dto);
    Task<StorageBoxResponseDto> FullUpdateAsync(string id, StorageBoxCreateDto dto);
    Task<StorageBoxResponseDto> PartialUpdateAsync(string id, StorageBoxPatchDto dto);
    Task<StorageBoxResponseDto> DisinfectAsync(string id);
    Task DeleteAsync(string id);
}