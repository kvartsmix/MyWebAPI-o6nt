using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Services;

public interface IStorageLeaseContractService
{
    Task<IReadOnlyList<StorageLeaseContractResponseDto>> GetAllAsync(int? count = null);
    Task<StorageLeaseContractResponseDto> GetByIdAsync(string id);
    Task<StorageLeaseContractResponseDto> CreateContractAsync(StorageLeaseContractCreateDto dto);
    Task<StorageLeaseContractResponseDto> PayDepositAsync(string id, decimal value);
    Task<string> GetPaymentNotificationAsync(string id);
    Task<StorageLeaseContractResponseDto> CheckOverdueAndSuspendAsync(string id);
    Task<StorageLeaseContractResponseDto> CloseContractAsync(string id);
    Task<StorageLeaseContractResponseDto> UpdateContractAsync(string id, StorageLeaseContractCreateDto dto);
    Task<StorageLeaseContractResponseDto> PatchContractAsync(string id, StorageLeaseContractPatchDto dto);
    Task DeleteContractAsync(string id);
}