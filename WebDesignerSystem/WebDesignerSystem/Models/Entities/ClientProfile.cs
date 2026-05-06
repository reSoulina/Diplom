using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class ClientProfile
    {
        [Key]
        public int Id { get; set; }

        // Уберите [Required] с этого поля или сделайте его nullable
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Полное имя")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Адрес доставки")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Display(Name = "Дата обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
    }
}