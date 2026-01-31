using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDesignerSystem.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Номер заказа")]
        public string OrderNumber { get; set; }

        [Display(Name = "Клиент")]
        public string ClientName { get; set; }

        [Display(Name = "Email клиента")]
        [EmailAddress]
        public string ClientEmail { get; set; }

        [Display(Name = "Дата заказа")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Сумма заказа")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Текущий статус")]
        public string CurrentStatus { get; set; }

        [Display(Name = "Цвет статуса")]
        public string StatusColor { get; set; }

        [Display(Name = "Примечания")]
        public string Notes { get; set; }

        [Display(Name = "Товары")]
        public List<OrderItemViewModel> OrderItems { get; set; }

        [Display(Name = "История статусов")]
        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; }

        // Для изменения статуса
        [Display(Name = "Новый статус")]
        public int NewStatusId { get; set; }

        [Display(Name = "Комментарий к изменению статуса")]
        [StringLength(500)]
        public string StatusChangeComment { get; set; }

        public OrderViewModel()
        {
            OrderItems = new List<OrderItemViewModel>();
            StatusHistory = new List<OrderStatusHistoryViewModel>();
        }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string ImageUrl { get; set; }
        public bool IsService { get; set; }
    }

    public class OrderStatusHistoryViewModel
    {
        public string StatusName { get; set; }
        public string ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Comment { get; set; }
        public string TimeAgo => GetTimeAgo(ChangedAt);

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return "только что";
            else if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{(int)timeSpan.TotalMinutes} минут назад";
            else if (timeSpan <= TimeSpan.FromHours(24))
                return $"{(int)timeSpan.TotalHours} часов назад";
            else
                return $"{(int)timeSpan.TotalDays} дней назад";
        }
    }
}