using MongoDB.Driver;
using MyWebAPI.Data;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public class StorageLeaseContractRepository : IStorageLeaseContractRepository
{
    private readonly IMongoCollection<StorageLeaseContractDocument> _collection;

    public StorageLeaseContractRepository(IMongoClient client)
    {
        var database = client.GetDatabase("mydatabase");
        _collection = database.GetCollection<StorageLeaseContractDocument>("lease_contracts");
    }

    public async Task<IReadOnlyList<StorageLeaseContract>> GetAllAsync()
    {
        var documents = await _collection.Find(_ => true).ToListAsync();
        return documents.Select(ToModel).ToList();
    }

    public async Task<StorageLeaseContract?> GetByIdAsync(string id)
    {
        var document = await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        return document is null ? null : ToModel(document);
    }

    public async Task<StorageLeaseContract?> GetActiveContractByBoxIdAsync(string boxId)
    {
        var document = await _collection
            .Find(x => x.StorageBoxId == boxId && x.Status != ContractStatus.Closed.ToString())
            .FirstOrDefaultAsync();

        return document is null ? null : ToModel(document);
    }

    public async Task<StorageLeaseContract> CreateAsync(StorageLeaseContract contract)
    {
        var document = new StorageLeaseContractDocument
        {
            StorageBoxId = contract.StorageBoxId,
            StorageTenantId = contract.StorageTenantId,
            DurationDays = contract.DurationDays,
            StartDate = contract.StartDate,
            Status = contract.Status.ToString(),
            MonthlyRate = contract.MonthlyRate,
            DepositAmount = contract.DepositAmount,
            IsDepositPaid = contract.IsDepositPaid,
            AccessPinCode = contract.AccessPinCode,
            IsPinActive = contract.IsPinActive
        };

        await _collection.InsertOneAsync(document);
        contract.Id = document.Id;
        return contract;
    }

    public async Task<bool> ReplaceAsync(string id, StorageLeaseContract contract)
    {
        var document = new StorageLeaseContractDocument
        {
            Id = id,
            StorageBoxId = contract.StorageBoxId,
            StorageTenantId = contract.StorageTenantId,
            DurationDays = contract.DurationDays,
            StartDate = contract.StartDate,
            Status = contract.Status.ToString(),
            MonthlyRate = contract.MonthlyRate,
            DepositAmount = contract.DepositAmount,
            IsDepositPaid = contract.IsDepositPaid,
            AccessPinCode = contract.AccessPinCode,
            IsPinActive = contract.IsPinActive
        };

        var result = await _collection.ReplaceOneAsync(x => x.Id == id, document);
        return result.MatchedCount > 0;
    }

    public async Task<StorageLeaseContract?> UpdateAsync(string id, StorageLeaseContractPatchDto dto)
    {
        var updates = new List<UpdateDefinition<StorageLeaseContractDocument>>();
        var update = Builders<StorageLeaseContractDocument>.Update;

        if (!string.IsNullOrWhiteSpace(dto.StorageTenantId))
            updates.Add(update.Set(x => x.StorageTenantId, dto.StorageTenantId));

        if (!string.IsNullOrWhiteSpace(dto.StorageBoxId))
            updates.Add(update.Set(x => x.StorageBoxId, dto.StorageBoxId));

        if (dto.DurationDays.HasValue)
            updates.Add(update.Set(x => x.DurationDays, dto.DurationDays.Value));

        if (updates.Count == 0)
            return await GetByIdAsync(id);

        var options = new FindOneAndUpdateOptions<StorageLeaseContractDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var document = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id,
            update.Combine(updates),
            options);

        return document is null ? null : ToModel(document);
    }

    public async Task<bool> ActivateContractAsync(string id, string pinCode)
    {
        var update = Builders<StorageLeaseContractDocument>.Update
            .Set(x => x.IsDepositPaid, true)
            .Set(x => x.Status, ContractStatus.Active.ToString())
            .Set(x => x.AccessPinCode, pinCode)
            .Set(x => x.IsPinActive, true);

        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> SuspendContractAsync(string id)
    {
        var update = Builders<StorageLeaseContractDocument>.Update
            .Set(x => x.Status, ContractStatus.Suspended.ToString())
            .Set(x => x.IsPinActive, false);

        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> CloseContractAsync(string id)
    {
        var update = Builders<StorageLeaseContractDocument>.Update
            .Set(x => x.Status, ContractStatus.Closed.ToString())
            .Set(x => x.IsPinActive, false);

        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    private static StorageLeaseContract ToModel(StorageLeaseContractDocument document) => new()
    {
        Id = document.Id,
        StorageBoxId = document.StorageBoxId,
        StorageTenantId = document.StorageTenantId,
        DurationDays = document.DurationDays,
        StartDate = document.StartDate,
        Status = Enum.Parse<ContractStatus>(document.Status),
        MonthlyRate = document.MonthlyRate,
        DepositAmount = document.DepositAmount,
        IsDepositPaid = document.IsDepositPaid,
        AccessPinCode = document.AccessPinCode,
        IsPinActive = document.IsPinActive
    };
}