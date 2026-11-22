using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Models
{
    public enum StorageBoxType
    {
        Heating,
        Unheating
    }

    public enum StorageBoxStatus
    {
        Available,
        Reserved,
        Occupied
    }

    public class StorageBox
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Площа є обов'язковою.")]
        [Range(0.5, 100, ErrorMessage = "Площа повинна бути в межах від 0.5 до 100 м2.")]
        public double Square { get; set; }

        [Required(ErrorMessage = "Об'єм є обов'язковим.")]
        [Range(1, 30, ErrorMessage = "Об'єм повинен бути в межах від 1 до 30 м3.")]
        public double Volume { get; set; }

        [Required(ErrorMessage = "Тип боксу є обов'язковим.")]
        public StorageBoxType Type { get; set; }

        public StorageBoxStatus Status { get; set; } = StorageBoxStatus.Available;

        [Required(ErrorMessage = "Статус дезінфекції є обов'язковим.")]
        public bool IsDisinfected { get; set; }

        [Required(ErrorMessage = "Місячна ціна є обов'язковою.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Місячна ціна має бути більшою за 0.")]
        public decimal MonthlyPrice { get; set; }

        public string? CurrentContractId { get; set; }
    }
}