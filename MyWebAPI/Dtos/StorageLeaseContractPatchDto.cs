using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Dtos
{
    public class StorageLeaseContractPatchDto
    {
        public string? StorageBoxId { get; set; }
        public string? StorageTenantId { get; set; }

        [Range(30, 3650, ErrorMessage = "Мінімальний термін оренди становить 30 днів.")]
        public int? DurationDays { get; set; }
    }
}
