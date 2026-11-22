namespace MyWebAPI.Models
{
    public enum TenantType
    {
        Individual,
        SmallBusiness
    }

    public class StorageTenant
    {
        public string? Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public TenantType? Type { get; set; }
        public bool HasDebt { get; set; }
        public List<string> ContractsId { get; set; } = new();
    }
}