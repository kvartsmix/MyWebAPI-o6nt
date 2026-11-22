using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public interface IStorageTenantRepository
{
    Task<IReadOnlyList<StorageTenant>> GetAllAsync();
    Task<StorageTenant?> GetByIdAsync(string id);
    Task<StorageTenant> CreateAsync(StorageTenantCreateDto dto);
    Task<bool> ReplaceAsync(string id, StorageTenant tenant);
    Task<StorageTenant?> UpdateAsync(string id, StorageTenantPatchDto dto);
    Task<bool> SetDebtAsync(string id, bool hasDebt);
    Task<bool> AddContractAsync(string tenantId, string contractId);
    Task<bool> RemoveContractAsync(string tenantId, string contractId);
    Task<bool> DeleteAsync(string id);
}