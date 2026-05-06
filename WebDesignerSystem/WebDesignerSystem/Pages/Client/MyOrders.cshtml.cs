using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Client
{
    [Authorize(Roles = "Client")]
    public class MyOrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public MyOrdersModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public List<Order> Orders { get; set; }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            Orders = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.CurrentStatus)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}