using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Client
{
    [Authorize(Roles = "Client")]
    public class MyAppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MyAppointmentsModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ServiceAppointment> Appointments { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Appointments = await _context.ServiceAppointments
                    .Include(a => a.Service)
                    .Where(a => a.ClientId == user.Id)
                    .OrderByDescending(a => a.AppointmentDateTime)   // самые ближайшие/новые сверху
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var appointment = await _context.ServiceAppointments
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == user.Id && a.Status == "pending");

            if (appointment != null)
            {
                appointment.Status = "cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запись отменена";
            }
            return RedirectToPage();
        }
    }
}