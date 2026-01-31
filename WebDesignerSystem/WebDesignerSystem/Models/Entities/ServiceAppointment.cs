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
        public string ClientId { get; set; } // ИЗМЕНЕНО: было int, стало string

        [Required]
        [ForeignKey("Service")]
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Дата и время записи")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDateTime { get; set; }

        [Display(Name = "Продолжительность (минуты)")]
        [Range(15, 480, ErrorMessage = "Продолжительность должна быть от 15 до 480 минут")]
        public int DurationMinutes { get; set; } = 60;

        [Required]
        [Display(Name = "Статус записи")]
        [StringLength(20)]
        public string Status { get; set; } = "booked";

        [Display(Name = "Примечания")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Дата создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата последнего обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Вычисляемое свойство
        [NotMapped]
        [Display(Name = "Время окончания")]
        public DateTime EndDateTime => AppointmentDateTime.AddMinutes(DurationMinutes);

        // Навигационные свойства
        [Display(Name = "Клиент")]
        public virtual User Client { get; set; }

        [Display(Name = "Услуга")]
        public virtual Product Service { get; set; }

        [NotMapped]
        public bool IsTimeSlotAvailable { get; set; } = true;
    }
}