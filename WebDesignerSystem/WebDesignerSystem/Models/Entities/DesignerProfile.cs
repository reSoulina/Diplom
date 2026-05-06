using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class DesignerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100)]
        [Display(Name = "Имя дизайнера")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Должность/опыт")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Bio { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Часы работы")]
        public string WorkingHours { get; set; } = string.Empty;

        [Display(Name = "URL фото")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Путь к файлу фото")]
        public string? PhotoPath { get; set; }

        [Display(Name = "Дата обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}