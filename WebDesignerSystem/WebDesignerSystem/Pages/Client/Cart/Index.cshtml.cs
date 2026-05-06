using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Client.Cart
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

        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            CartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            TotalAmount = CartItems.Sum(c => c.Product.Price * c.Quantity);
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Товар удалён из корзины";
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int id, int quantity)
        {
            if (quantity < 1) quantity = 1;
            if (quantity > 99) quantity = 99;

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

                if (item != null)
                {
                    item.Quantity = quantity;
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Количество обновлено";
                }
            }
            return RedirectToPage();
        }
    }
}