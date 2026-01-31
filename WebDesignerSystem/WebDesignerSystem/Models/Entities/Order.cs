using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        [Display(Name = "Клиент")]
        public string ClientId { get; set; } // ИЗМЕНЕНО: было int, стало string

        [Required]
        [Display(Name = "Дата заказа")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Сумма заказа")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("CurrentStatus")]
        [Display(Name = "Текущий статус")]
        public int CurrentStatusId { get; set; } = 1;

        [Display(Name = "Примечания")]
        public string? Notes { get; set; }

        // Навигационные свойства
        public virtual User Client { get; set; }
        public virtual OrderStatus CurrentStatus { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; }

        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
            StatusHistory = new HashSet<OrderStatusHistory>();
        }
    }
}