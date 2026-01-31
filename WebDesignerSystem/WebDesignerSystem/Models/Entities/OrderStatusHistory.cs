using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Order")]
        [Display(Name = "Заказ")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Status")]
        [Display(Name = "Статус")]
        public int StatusId { get; set; }

        [Required]
        [Display(Name = "Дата изменения")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("ChangedByUser")]
        [Display(Name = "Кто изменил")]
        public string ChangedBy { get; set; } // ИЗМЕНЕНО: было int, стало string

        [Display(Name = "Комментарий")]
        [StringLength(500)]
        public string Comment { get; set; }

        // Навигационные свойства
        public virtual Order Order { get; set; }
        public virtual OrderStatus Status { get; set; }

        [Display(Name = "Пользователь, изменивший статус")]
        public virtual User ChangedByUser { get; set; }
    }
}