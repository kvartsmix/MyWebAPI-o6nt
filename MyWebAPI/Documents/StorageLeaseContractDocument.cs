using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyWebAPI.Documents;

public class StorageLeaseContractDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string StorageBoxId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string StorageTenantId { get; set; } = string.Empty;

    public int DurationDays { get; set; }
    public DateTime StartDate { get; set; }

    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal MonthlyRate { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal DepositAmount { get; set; }

    public bool IsDepositPaid { get; set; }
    public string? AccessPinCode { get; set; }
    public bool IsPinActive { get; set; }
}