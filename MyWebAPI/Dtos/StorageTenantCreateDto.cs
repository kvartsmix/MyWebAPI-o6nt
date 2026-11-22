using MyWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Dtos
{
    public class StorageTenantCreateDto
    {
        [Required(ErrorMessage = "Повне ім'я є обов'язковим.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Повне ім'я повинно містити від 3 до 100 символів.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер телефону є обов'язковим.")]
        [Phone(ErrorMessage = "Невірний формат номера телефону.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Електронна пошта є обов'язковою.")]
        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип орендаря є обов'язковим.")]
        [EnumDataType(typeof(TenantType), ErrorMessage = "Невірний тип орендаря.")]
        public TenantType Type { get; set; }
    }
}
