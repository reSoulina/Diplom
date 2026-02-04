using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.ViewModels;
using WebDesignerSystem.Services;

namespace WebDesignerSystem.Pages.Admin.Products
{
    [Authorize(Roles = "Designer")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public CreateModel(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [BindProperty]
        public ProductViewModel Product { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCategories();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategories();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string imageUrl = null;
                if (Product.ImageFile != null && Product.ImageFile.Length > 0)
                {
                    try
                    {
                        imageUrl = await _fileService.SaveImageAsync(Product.ImageFile);
                    }
                    catch
                    {
                        imageUrl = null;
                    }
                }

                var newProduct = new Models.Entities.Product
                {
                    Name = Product.Name,
                    Description = Product.Description,
                    Price = Product.Price,
                    CategoryId = Product.CategoryId,
                    IsService = Product.ProductType == "service",
                    IsActive = Product.IsActive,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Товар '{Product.Name}' успешно добавлен!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadCategories()
        {
            if (Product == null)
            {
                Product = new ProductViewModel();
            }

            var categories = await _context.Categories
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            Product.Categories = categories;
        }
    }
}