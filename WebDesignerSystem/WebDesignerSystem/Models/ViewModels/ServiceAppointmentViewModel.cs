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
        public int ServiceId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(10, 0, 0);
        [Range(15, 480)]
        public int DurationMinutes { get; set; } = 60;
        public string Notes { get; set; }
        public string ClientName { get; set; }
        public string ServiceName { get; set; }
        public List<TimeSlot> AvailableTimeSlots { get; set; } = new();
        [NotMapped]
        public DateTime AppointmentDateTime
        {
            get => AppointmentDate.Add(AppointmentTime);
            set { AppointmentDate = value.Date; AppointmentTime = value.TimeOfDay; }
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