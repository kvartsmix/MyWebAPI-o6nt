using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MyWebAPI.Models
{
    public enum ContractStatus
    {
        PendingDeposit,
        Active,
        Suspended,
        Closed
    }

    public class StorageLeaseContract
    {
        public string? Id { get; set; }
        public string StorageBoxId { get; set; } = string.Empty;
        public string StorageTenantId { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public DateTime StartDate { get; set; }
        public ContractStatus Status { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal DepositAmount { get; set; }
        public bool IsDepositPaid { get; set; }
        public string? AccessPinCode { get; set; }
        public bool IsPinActive { get; set; }
        public int DiscountPercentage => DurationDays >= 180 ? 10 : 0;
        public DateTime ReservedUntil => StartDate.AddHours(24);
        public DateTime EndDate => StartDate.AddDays(DurationDays);
        public DateTime NextPaymentDueDate => StartDate.AddDays(30);
        public DateTime InvoiceIssueDate => NextPaymentDueDate.AddDays(-5);
        public bool IsExpired() => DateTime.UtcNow > NextPaymentDueDate.AddDays(3);
        public bool IsInvoice() => DateTime.UtcNow > InvoiceIssueDate;
    }

}