using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Client.Profile
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public ClientProfile Profile { get; set; } = new ClientProfile();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var profile = await _context.ClientProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                Profile = new ClientProfile
                {
                    UserId = user.Id,
                    FullName = user.FullName ?? "",
                    Phone = "",
                    DeliveryAddress = ""
                };
            }
            else
            {
                Profile = profile;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Принудительно устанавливаем UserId
            Profile.UserId = user.Id;

            // Убираем проверку валидации для UserId (если нужно)
            ModelState.Remove("Profile.UserId");
            ModelState.Remove("Profile.User");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            Profile.UpdatedAt = DateTime.UtcNow;

            if (existingProfile == null)
            {
                _context.ClientProfiles.Add(Profile);
            }
            else
            {
                existingProfile.FullName = Profile.FullName;
                existingProfile.Phone = Profile.Phone;
                existingProfile.DeliveryAddress = Profile.DeliveryAddress;
                existingProfile.UpdatedAt = Profile.UpdatedAt;
            }

            await _context.SaveChangesAsync();

            user.FullName = Profile.FullName;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Профиль успешно обновлён!";
            return RedirectToPage();
        }
    }
}