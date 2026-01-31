using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Product")]
        [Display(Name = "Товар")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Количество")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Цена за единицу")]
        public decimal UnitPrice { get; set; }

        // Навигационные свойства
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}