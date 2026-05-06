using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Profile
{
    [Authorize(Roles = "Designer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public DesignerProfile Profile { get; set; } = new DesignerProfile();

        [BindProperty]
        public IFormFile? PhotoFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var profile = await _context.DesignerProfiles.FirstOrDefaultAsync();
            if (profile != null)
            {
                Profile = profile;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingProfile = await _context.DesignerProfiles.FirstOrDefaultAsync();
            Profile.UpdatedAt = DateTime.UtcNow;

            // Обработка загрузки фото
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "designer");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Удаляем старое фото
                if (!string.IsNullOrEmpty(existingProfile?.PhotoPath) && System.IO.File.Exists(existingProfile.PhotoPath))
                {
                    System.IO.File.Delete(existingProfile.PhotoPath);
                }

                var uniqueFileName = $"designer_{DateTime.Now.Ticks}{Path.GetExtension(PhotoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }

                Profile.PhotoPath = filePath;
                Profile.PhotoUrl = $"/images/designer/{uniqueFileName}";
            }
            else if (existingProfile != null)
            {
                Profile.PhotoPath = existingProfile.PhotoPath;
                Profile.PhotoUrl = existingProfile.PhotoUrl;
            }

            if (existingProfile == null)
            {
                _context.DesignerProfiles.Add(Profile);
            }
            else
            {
                existingProfile.Name = Profile.Name;
                existingProfile.Position = Profile.Position;
                existingProfile.Bio = Profile.Bio;
                existingProfile.Email = Profile.Email;
                existingProfile.Phone = Profile.Phone;
                existingProfile.WorkingHours = Profile.WorkingHours;
                existingProfile.PhotoPath = Profile.PhotoPath;
                existingProfile.PhotoUrl = Profile.PhotoUrl;
                existingProfile.UpdatedAt = Profile.UpdatedAt;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Профиль успешно обновлён!";
            return RedirectToPage();
        }
    }
}