using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public DesignerProfile? DesignerProfile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // ≈сли пользователь авторизован, перенаправл€ем в зависимости от роли
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var role = await _context.Roles.FindAsync(user.RoleId);
                    if (role?.Name == "Designer")
                        return RedirectToPage("/Admin/Index");
                }
            }

            // ƒл€ неавторизованных пользователей загружаем профиль дизайнера (как обычно)
            DesignerProfile = await _context.DesignerProfiles.FirstOrDefaultAsync();
            return Page();
        }
    }
}