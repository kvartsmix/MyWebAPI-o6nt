using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Dtos
{
    public class StorageLeaseContractCreateDto
    {
        [Required(ErrorMessage = "ID боксу є обов'язковим.")]
        public string StorageBoxId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID орендаря є обов'язковим.")]
        public string StorageTenantId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Термін оренди є обов'язковим.")]
        [Range(30, 3650, ErrorMessage = "Мінімальний термін оренди становить 30 днів.")]
        public int DurationDays { get; set; }
    }
}
