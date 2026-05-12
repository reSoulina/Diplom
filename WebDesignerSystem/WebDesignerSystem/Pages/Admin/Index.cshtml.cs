// Pages/Admin/Index.cshtml.cs (уже обновлён)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin
{
    [Authorize(Roles = "Designer")]
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
        public List<ServiceAppointment> RecentAppointments { get; set; }
        public List<Order> ActiveOrders { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                DesignerProfile = await _context.DesignerProfiles.FirstOrDefaultAsync();
            }

            // Ближайшие записи (не отменённые, начиная с текущего времени)
            RecentAppointments = await _context.ServiceAppointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDateTime >= DateTime.UtcNow && a.Status != "cancelled")
                .OrderBy(a => a.AppointmentDateTime)
                .Take(5)
                .ToListAsync();

            // Активные заказы (статусы "В очереди" и "В процессе")
            int[] activeStatusIds = { 1, 2 };
            ActiveOrders = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.CurrentStatus)
                .Where(o => activeStatusIds.Contains(o.CurrentStatusId))
                .OrderBy(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
        }
    }
}