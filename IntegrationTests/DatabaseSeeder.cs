using MongoDB.Bson;
using MongoDB.Driver;
using MyWebAPI.Documents;
using MyWebAPI.Models;

namespace IntegrationTests;

public class DatabaseSeeder
{
    public const string CollectionName = "storage_boxes";
    private readonly IMongoDatabase _db;

    public DatabaseSeeder(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task SeedStorageBoxesAsync()
    {
        var collection = _db.GetCollection<StorageBoxDocument>(CollectionName);

        await collection.DeleteManyAsync(FilterDefinition<StorageBoxDocument>.Empty);

        var boxes = new List<StorageBoxDocument>
        {
            new StorageBoxDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Square = 10.0,
                Volume = 15.0,
                Type = StorageBoxType.Heating.ToString(),
                Status = StorageBoxStatus.Available.ToString(),
                IsDisinfected = true,
                MonthlyPrice = 1500m
            },
            new StorageBoxDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Square = 5.5,
                Volume = 8.0,
                Type = StorageBoxType.Unheating.ToString(),
                Status = StorageBoxStatus.Available.ToString(),
                IsDisinfected = false,
                MonthlyPrice = 800m
            },
            new StorageBoxDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Square = 20.0,
                Volume = 25.0,
                Type = StorageBoxType.Heating.ToString(),
                Status = StorageBoxStatus.Occupied.ToString(),
                IsDisinfected = true,
                MonthlyPrice = 3000m
            }
        };

        await collection.InsertManyAsync(boxes);
    }
}