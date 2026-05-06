using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Appointments
{
    [Authorize(Roles = "Designer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ServiceAppointment> Appointments { get; set; }

        public async Task OnGetAsync()
        {
            Appointments = await _context.ServiceAppointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();
        }
    }
}