using MongoDB.Driver;

namespace MyWebAPI.Data;

public class MongoDbClient
{
    private static IMongoDatabase _db;
    private static MongoDbClient _instance;

    public static MongoDbClient Instance
    {
        get => _instance ??= new MongoDbClient();
    }

    private MongoDbClient()
    {
        var connectionString = "mongodb+srv://dentertop224_db_user:KNtD0AuJK7Ck9Cmw@lw4.lpeukwv.mongodb.net/";
        var client = new MongoClient(connectionString);
        _db = client.GetDatabase("mydatabase");
    }

    public static void UseTestDatabase(IMongoDatabase testDb)
    {
        _instance ??= new MongoDbClient();
        _db = testDb;
    }

    public IMongoCollection<T> GetCollection<T>(string name) => _db.GetCollection<T>(name);
}