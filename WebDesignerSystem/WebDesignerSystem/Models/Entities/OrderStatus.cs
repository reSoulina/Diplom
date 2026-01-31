using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDesignerSystem.Models.Entities
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Название статуса")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Порядок отображения")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Цвет статуса")]
        [StringLength(20)]
        public string Color { get; set; } = "#000000";

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; }

        public OrderStatus()
        {
            Orders = new HashSet<Order>();
            StatusHistories = new HashSet<OrderStatusHistory>();
        }
    }
}