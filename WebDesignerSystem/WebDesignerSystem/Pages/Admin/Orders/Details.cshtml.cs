using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Orders
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
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<OrderStatus> Statuses { get; set; }
        public List<OrderStatusHistory> StatusHistory { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders.Include(o => o.Client).Include(o => o.CurrentStatus).FirstOrDefaultAsync(o => o.Id == id);
            if (Order == null) return NotFound();
            OrderItems = await _context.OrderItems.Include(oi => oi.Product).Where(oi => oi.OrderId == id).ToListAsync();
            Statuses = await _context.OrderStatuses.OrderBy(s => s.DisplayOrder).ToListAsync();
            StatusHistory = await _context.OrderStatusHistories
                .Include(h => h.Status)
                .Include(h => h.ChangedByUser)
                .Where(h => h.OrderId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostChangeStatusAsync(int id, int newStatusId, string comment)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            order.CurrentStatusId = newStatusId;
            order.UpdatedAt = DateTime.UtcNow;
            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                StatusId = newStatusId,
                ChangedBy = user.Id,
                Comment = comment
            };
            _context.OrderStatusHistories.Add(history);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Статус обновлён";
            return RedirectToPage(new { id });
        }
    }
}