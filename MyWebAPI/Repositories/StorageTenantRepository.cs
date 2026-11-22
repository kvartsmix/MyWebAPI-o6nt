using MongoDB.Driver;
using MyWebAPI.Data;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public class StorageTenantRepository : IStorageTenantRepository
{
    private readonly IMongoCollection<StorageTenantDocument> _collection;

    public StorageTenantRepository(IMongoClient client)
    {
        var database = client.GetDatabase("mydatabase");
        _collection = database.GetCollection<StorageTenantDocument>("tenants");
    }

    public async Task<IReadOnlyList<StorageTenant>> GetAllAsync()
    {
        var documents = await _collection.Find(_ => true).ToListAsync();
        return documents.Select(ToModel).ToList();
    }

    public async Task<StorageTenant?> GetByIdAsync(string id)
    {
        var document = await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        return document is null ? null : ToModel(document);
    }

    public async Task<StorageTenant> CreateAsync(StorageTenantCreateDto dto)
    {
        var document = new StorageTenantDocument
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Type = dto.Type.ToString(),
            HasDebt = false,
            ContractsId = new List<string>()
        };

        await _collection.InsertOneAsync(document);
        return ToModel(document);
    }

    public async Task<bool> ReplaceAsync(string id, StorageTenant tenant)
    {
        var document = new StorageTenantDocument
        {
            Id = id,
            FullName = tenant.FullName,
            PhoneNumber = tenant.PhoneNumber,
            Email = tenant.Email,
            Type = tenant.Type?.ToString(),
            HasDebt = tenant.HasDebt,
            ContractsId = tenant.ContractsId
        };

        var result = await _collection.ReplaceOneAsync(x => x.Id == id, document);
        return result.MatchedCount > 0;
    }

    public async Task<StorageTenant?> UpdateAsync(string id, StorageTenantPatchDto dto)
    {
        var updates = new List<UpdateDefinition<StorageTenantDocument>>();
        var update = Builders<StorageTenantDocument>.Update;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            updates.Add(update.Set(x => x.FullName, dto.FullName));

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            updates.Add(update.Set(x => x.PhoneNumber, dto.PhoneNumber));

        if (!string.IsNullOrWhiteSpace(dto.Email))
            updates.Add(update.Set(x => x.Email, dto.Email));

        if (dto.Type.HasValue)
            updates.Add(update.Set(x => x.Type, dto.Type.Value.ToString()));

        if (updates.Count == 0)
            return await GetByIdAsync(id);

        var options = new FindOneAndUpdateOptions<StorageTenantDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var document = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id,
            update.Combine(updates),
            options);

        return document is null ? null : ToModel(document);
    }

    public async Task<bool> SetDebtAsync(string id, bool hasDebt)
    {
        var update = Builders<StorageTenantDocument>.Update.Set(x => x.HasDebt, hasDebt);
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> AddContractAsync(string tenantId, string contractId)
    {
        var update = Builders<StorageTenantDocument>.Update.AddToSet(x => x.ContractsId, contractId);
        var result = await _collection.UpdateOneAsync(x => x.Id == tenantId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> RemoveContractAsync(string tenantId, string contractId)
    {
        var update = Builders<StorageTenantDocument>.Update.Pull(x => x.ContractsId, contractId);
        var result = await _collection.UpdateOneAsync(x => x.Id == tenantId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    private static StorageTenant ToModel(StorageTenantDocument document) => new()
    {
        Id = document.Id,
        FullName = document.FullName,
        PhoneNumber = document.PhoneNumber,
        Email = document.Email,
        Type = !string.IsNullOrEmpty(document.Type) ? Enum.Parse<TenantType>(document.Type) : null,
        HasDebt = document.HasDebt,
        ContractsId = document.ContractsId
    };
}