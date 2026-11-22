using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Services;

public interface IStorageTenantService
{
    Task<IReadOnlyList<StorageTenantResponseDto>> GetAllAsync(int? count = null);
    Task<StorageTenantResponseDto> GetByIdAsync(string id);
    Task<StorageTenantResponseDto> CreateAsync(StorageTenantCreateDto dto);
    Task<StorageTenantResponseDto> FullUpdateAsync(string id, StorageTenantCreateDto dto);
    Task<StorageTenantResponseDto> PartialUpdateAsync(string id, StorageTenantPatchDto dto);
    Task<StorageTenantResponseDto> PayDebtAsync(string id);
    Task DeleteAsync(string id);
}