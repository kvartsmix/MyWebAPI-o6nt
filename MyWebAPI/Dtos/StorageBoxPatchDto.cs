using MyWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Dtos
{
    public class StorageBoxPatchDto
    {
        [Range(0.5, 100, ErrorMessage = "Площа повинна бути в межах від 0.5 до 100 м2.")]
        public double? Square { get; set; }

        [Range(1, 30, ErrorMessage = "Об'єм повинен бути в межах від 1 до 30 м3.")]
        public double? Volume { get; set; }

        [EnumDataType(typeof(StorageBoxType), ErrorMessage = "Невірний тип сховища.")]
        public StorageBoxType? Type { get; set; }

        public bool? IsDisinfected { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Місячна ціна має бути більшою за 0.")]
        public decimal? MonthlyPrice { get; set; }
    }
}
