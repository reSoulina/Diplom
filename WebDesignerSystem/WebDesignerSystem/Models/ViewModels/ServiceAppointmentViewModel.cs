using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.ViewModels
{
    public class ServiceAppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Время")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(10, 0, 0);

        [Display(Name = "Продолжительность (минуты)")]
        [Range(15, 480, ErrorMessage = "Продолжительность должна быть от 15 до 480 минут")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "Примечания")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Клиент")]
        public string ClientName { get; set; }

        [Display(Name = "Услуга")]
        public string ServiceName { get; set; }

        // Для отображения доступных слотов времени
        [Display(Name = "Доступное время")]
        public List<TimeSlot> AvailableTimeSlots { get; set; }

        // Комбинированное свойство для DateTime
        [NotMapped]
        public DateTime AppointmentDateTime
        {
            get => AppointmentDate.Add(AppointmentTime);
            set
            {
                AppointmentDate = value.Date;
                AppointmentTime = value.TimeOfDay;
            }
        }

        public ServiceAppointmentViewModel()
        {
            AvailableTimeSlots = new List<TimeSlot>();
        }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string DisplayText => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}