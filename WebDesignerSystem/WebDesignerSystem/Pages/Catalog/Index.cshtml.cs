using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Product> Products { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ProductType { get; set; }

        public async Task OnGetAsync()
        {
            // Получаем все активные продукты
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Фильтрация по категории
            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            // Фильтрация по типу (товар/услуга)
            if (!string.IsNullOrEmpty(ProductType))
            {
                if (ProductType == "product")
                {
                    query = query.Where(p => !p.IsService);
                }
                else if (ProductType == "service")
                {
                    query = query.Where(p => p.IsService);
                }
            }

            Products = await query
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Заполняем список категорий для фильтра
            Categories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id)
        {
            // Проверяем, авторизован ли пользователь
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = "/Catalog" });
            }

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Находим товар
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Товар не найден";
                return RedirectToPage();
            }

            // Проверяем, что это товар, а не услуга
            if (product.IsService)
            {
                TempData["ErrorMessage"] = "Это услуга. Запишитесь на неё через форму записи.";
                return RedirectToPage();
            }

            // Проверяем, активен ли товар
            if (!product.IsActive)
            {
                TempData["ErrorMessage"] = "Этот товар временно недоступен";
                return RedirectToPage();
            }

            // Ищем товар в корзине пользователя
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == id);

            if (cartItem == null)
            {
                // Добавляем новый товар в корзину
                cartItem = new CartItem
                {
                    UserId = user.Id,
                    ProductId = id,
                    Quantity = 1,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
                TempData["SuccessMessage"] = "Товар добавлен в корзину";
            }
            else
            {
                // Увеличиваем количество существующего товара
                cartItem.Quantity++;
                TempData["SuccessMessage"] = "Количество товара увеличено";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}