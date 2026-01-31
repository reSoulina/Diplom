using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDesignerSystem.Models.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; } // Изменено на nullable

        // Навигационное свойство
        public virtual ICollection<User> Users { get; set; }

        public Role()
        {
            Users = new HashSet<User>();
        }
    }
}