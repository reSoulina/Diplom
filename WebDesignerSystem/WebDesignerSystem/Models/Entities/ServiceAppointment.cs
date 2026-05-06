// Models/Entities/ServiceAppointment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class ServiceAppointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        [Display(Name = "Клиент")]
        public string ClientId { get; set; }

        [Required]
        [ForeignKey("Service")]
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Дата и время")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDateTime { get; set; }

        [Range(15, 480)]
        [Display(Name = "Длительность (мин)")]
        public int DurationMinutes { get; set; } = 60;

        [Required]
        [StringLength(20)]
        [Display(Name = "Статус")]
        public string Status { get; set; } = "pending"; // pending, confirmed, completed, cancelled

        [Required]
        [StringLength(20)]
        [Display(Name = "Формат")]
        public string? Format { get; set; }          // было string Format

        [Display(Name = "Контакт (для онлайн)")]
        [StringLength(100)]
        public string? ContactInfo { get; set; }     // было string ContactInfo

        [Display(Name = "Адрес (для офлайн)")]
        [StringLength(200)]
        public string? Address { get; set; }         // было string Address

        [Display(Name = "Примечания")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual User Client { get; set; }
        public virtual Product Service { get; set; }

        [NotMapped]
        public DateTime EndDateTime => AppointmentDateTime.AddMinutes(DurationMinutes);
    }
}