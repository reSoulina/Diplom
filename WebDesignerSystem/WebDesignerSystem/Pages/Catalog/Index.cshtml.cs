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

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = "/Catalog" });
            }

            // Здесь будет логика добавления в корзину
            TempData["Message"] = "Товар добавлен в корзину";
            return RedirectToPage();
        }
    }
}