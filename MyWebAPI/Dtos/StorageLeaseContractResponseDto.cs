namespace MyWebAPI.Dtos;

public class StorageLeaseContractResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string StorageBoxId { get; set; } = string.Empty;
    public string StorageTenantId { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal MonthlyRate { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsDepositPaid { get; set; }
    public string? AccessPinCode { get; set; }
    public bool IsPinActive { get; set; }

    public int DiscountPercentage { get; set; }
    public DateTime ReservedUntil { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime NextPaymentDueDate { get; set; }
}