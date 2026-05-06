using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Appointments
{
    [Authorize(Roles = "Designer")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ServiceAppointment Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Appointment = await _context.ServiceAppointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (Appointment == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(int id, string newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var appointment = await _context.ServiceAppointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус записи обновлён";
            return RedirectToPage(new { id });
        }
    }
}