using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public interface IStorageLeaseContractRepository
{
    Task<IReadOnlyList<StorageLeaseContract>> GetAllAsync();
    Task<StorageLeaseContract?> GetByIdAsync(string id);
    Task<StorageLeaseContract?> GetActiveContractByBoxIdAsync(string boxId);
    Task<StorageLeaseContract> CreateAsync(StorageLeaseContract contract);
    Task<bool> ReplaceAsync(string id, StorageLeaseContract contract);
    Task<StorageLeaseContract?> UpdateAsync(string id, StorageLeaseContractPatchDto dto);
    Task<bool> ActivateContractAsync(string id, string pinCode);
    Task<bool> SuspendContractAsync(string id);
    Task<bool> CloseContractAsync(string id);
    Task<bool> DeleteAsync(string id);
}