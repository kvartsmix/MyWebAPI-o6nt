using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyWebAPI.Documents;

public class StorageTenantDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public string? Type { get; set; }

    public bool HasDebt { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ContractsId { get; set; } = new();
}