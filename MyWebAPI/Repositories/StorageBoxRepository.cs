using MongoDB.Driver;
using MyWebAPI.Data;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public class StorageBoxRepository : IStorageBoxRepository
{
    private readonly IMongoCollection<StorageBoxDocument> _collection;
        
    public StorageBoxRepository(IMongoClient client)
    {
        var database = client.GetDatabase("mydatabase");
        _collection = database.GetCollection<StorageBoxDocument>("storage_boxes");
    }

    public async Task<IReadOnlyList<StorageBox>> GetAllAsync()
    {
        var documents = await _collection.Find(_ => true).ToListAsync();
        return documents.Select(ToModel).ToList();
    }

    public async Task<StorageBox?> GetByIdAsync(string id)
    {
        var document = await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        return document is null ? null : ToModel(document);
    }

    public async Task<IReadOnlyList<StorageBox>> GetAvailableAsync(StorageBoxType? type = null, double? minVolume = null)
    {
        var filterBuilder = Builders<StorageBoxDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.Status, StorageBoxStatus.Available.ToString()) &
                     filterBuilder.Eq(x => x.IsDisinfected, true);

        if (type.HasValue)
            filter &= filterBuilder.Eq(x => x.Type, type.Value.ToString());

        if (minVolume.HasValue)
            filter &= filterBuilder.Gte(x => x.Volume, minVolume.Value);

        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Select(ToModel).ToList();
    }

    public async Task<StorageBox> CreateAsync(StorageBoxCreateDto dto)
    {
        var document = new StorageBoxDocument
        {
            Square = dto.Square,
            Volume = dto.Volume,
            Type = dto.Type.ToString(),
            Status = StorageBoxStatus.Available.ToString(),
            IsDisinfected = dto.IsDisinfected,
            MonthlyPrice = dto.MonthlyPrice,
            CurrentContractId = null
        };

        await _collection.InsertOneAsync(document);
        return ToModel(document);
    }

    public async Task<bool> ReplaceAsync(string id, StorageBox box)
    {
        var document = new StorageBoxDocument
        {
            Id = id,
            Square = box.Square,
            Volume = box.Volume,
            Type = box.Type.ToString(),
            Status = box.Status.ToString(),
            IsDisinfected = box.IsDisinfected,
            MonthlyPrice = box.MonthlyPrice,
            CurrentContractId = box.CurrentContractId
        };

        var result = await _collection.ReplaceOneAsync(x => x.Id == id, document);
        return result.MatchedCount > 0;
    }

    public async Task<StorageBox?> UpdateAsync(string id, StorageBoxPatchDto dto)
    {
        var updates = new List<UpdateDefinition<StorageBoxDocument>>();
        var update = Builders<StorageBoxDocument>.Update;

        if (dto.Square.HasValue)
            updates.Add(update.Set(x => x.Square, dto.Square.Value));

        if (dto.Volume.HasValue)
            updates.Add(update.Set(x => x.Volume, dto.Volume.Value));

        if (dto.Type.HasValue)
            updates.Add(update.Set(x => x.Type, dto.Type.Value.ToString()));

        if (dto.IsDisinfected.HasValue)
            updates.Add(update.Set(x => x.IsDisinfected, dto.IsDisinfected.Value));

        if (dto.MonthlyPrice.HasValue)
            updates.Add(update.Set(x => x.MonthlyPrice, dto.MonthlyPrice.Value));

        if (updates.Count == 0)
            return await GetByIdAsync(id);

        var options = new FindOneAndUpdateOptions<StorageBoxDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var document = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id,
            update.Combine(updates),
            options);

        return document is null ? null : ToModel(document);
    }

    public async Task<bool> UpdateStatusAndDisinfectionAsync(string id, StorageBoxStatus? status, bool? isDisinfected)
    {
        var updates = new List<UpdateDefinition<StorageBoxDocument>>();
        var update = Builders<StorageBoxDocument>.Update;

        if (status.HasValue)
            updates.Add(update.Set(x => x.Status, status.Value.ToString()));

        if (isDisinfected.HasValue)
            updates.Add(update.Set(x => x.IsDisinfected, isDisinfected.Value));

        if (updates.Count == 0) return true;

        var result = await _collection.UpdateOneAsync(x => x.Id == id, update.Combine(updates));
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    private static StorageBox ToModel(StorageBoxDocument document) => new()
    {
        Id = document.Id,
        Square = document.Square,
        Volume = document.Volume,
        Type = Enum.Parse<StorageBoxType>(document.Type),
        Status = Enum.Parse<StorageBoxStatus>(document.Status),
        IsDisinfected = document.IsDisinfected,
        MonthlyPrice = document.MonthlyPrice,
        CurrentContractId = document.CurrentContractId
    };
}