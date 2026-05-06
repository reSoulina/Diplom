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
    public class OrderDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public OrderDetailsModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");
            Order = await _context.Orders.Include(o => o.CurrentStatus).FirstOrDefaultAsync(o => o.Id == id && o.ClientId == user.Id);
            if (Order == null) return NotFound();
            OrderItems = await _context.OrderItems.Include(oi => oi.Product).Where(oi => oi.OrderId == id).ToListAsync();
            return Page();
        }
    }
}