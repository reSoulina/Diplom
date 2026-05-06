using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Orders
{
    [Authorize(Roles = "Designer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;
        public List<Order> Orders { get; set; }
        public async Task OnGetAsync()
        {
            Orders = await _context.Orders.Include(o => o.Client).Include(o => o.CurrentStatus).OrderByDescending(o => o.OrderDate).ToListAsync();
        }
    }
}