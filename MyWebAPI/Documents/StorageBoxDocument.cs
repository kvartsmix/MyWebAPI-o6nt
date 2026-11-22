using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyWebAPI.Documents;

public class StorageBoxDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public double Square { get; set; }
    public double Volume { get; set; }

    [BsonRepresentation(BsonType.String)]
    public string Type { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = string.Empty;

    public bool IsDisinfected { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal MonthlyPrice { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? CurrentContractId { get; set; }
}