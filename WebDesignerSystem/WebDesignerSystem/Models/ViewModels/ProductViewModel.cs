using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebDesignerSystem.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        [Display(Name = "Цена")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public string ProductType { get; set; } = "product";

        [Display(Name = "Активно")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Изображение")]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; } // Добавлен знак ?

        public string? ExistingImageUrl { get; set; } // Добавлен знак ?

        public List<SelectListItem> Categories { get; set; } = new();

        public bool IsService => ProductType == "service";
    }
}