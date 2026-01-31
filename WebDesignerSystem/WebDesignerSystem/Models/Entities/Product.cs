using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Это услуга?")]
        public bool IsService { get; set; } = false;

        [Required]
        [Display(Name = "Активно")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual Category Category { get; set; }
    }
}