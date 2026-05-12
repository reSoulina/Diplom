using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDesignerSystem.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string ClientName { get; set; }
        [EmailAddress]
        public string ClientEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrentStatus { get; set; }
        public string StatusColor { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; }
        public int NewStatusId { get; set; }
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
            if (timeSpan <= TimeSpan.FromSeconds(60)) return "только что";
            if (timeSpan <= TimeSpan.FromMinutes(60)) return $"{(int)timeSpan.TotalMinutes} минут назад";
            if (timeSpan <= TimeSpan.FromHours(24)) return $"{(int)timeSpan.TotalHours} часов назад";
            return $"{(int)timeSpan.TotalDays} дней назад";
        }
    }
}