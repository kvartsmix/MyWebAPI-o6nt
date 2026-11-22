using MyWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Dtos
{
    public class StorageTenantPatchDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Повне ім'я повинно містити від 3 до 100 символів.")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Невірний формат номера телефону.")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти.")]
        public string? Email { get; set; }

        [EnumDataType(typeof(TenantType), ErrorMessage = "Невірний тип орендаря.")]
        public TenantType? Type { get; set; }
    }
}
