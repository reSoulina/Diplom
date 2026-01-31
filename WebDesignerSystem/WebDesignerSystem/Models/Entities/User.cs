using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDesignerSystem.Models.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "Полное имя")]
        [StringLength(100)]
        public string FullName { get; set; }

        [ForeignKey("Role")]
        [Display(Name = "Роль")]
        public int RoleId { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual Role Role { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ServiceAppointment> Appointments { get; set; }

        public User()
        {
            Orders = new HashSet<Order>();
            Appointments = new HashSet<ServiceAppointment>();
        }
    }
}