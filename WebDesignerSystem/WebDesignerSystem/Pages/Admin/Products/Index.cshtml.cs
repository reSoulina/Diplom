using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Products
{
    [Authorize(Roles = "Designer")]
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
            await LoadProducts();
            await LoadCategories();
        }

        private async Task LoadProducts()
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

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
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        private async Task LoadCategories()
        {
            Categories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}