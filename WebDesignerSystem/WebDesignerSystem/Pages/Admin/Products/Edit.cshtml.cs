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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public EditModel(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [BindProperty]
        public ProductViewModel Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var productEntity = await _context.Products.FindAsync(id);
            if (productEntity == null)
            {
                return NotFound();
            }

            Product = new ProductViewModel
            {
                Id = productEntity.Id,
                Name = productEntity.Name,
                Description = productEntity.Description,
                Price = productEntity.Price,
                CategoryId = productEntity.CategoryId,
                ProductType = productEntity.IsService ? "service" : "product",
                IsActive = productEntity.IsActive,
                ExistingImageUrl = _fileService.GetSafeImageUrl(productEntity.ImageUrl)
            };

            await LoadCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return Page();
            }

            try
            {
                var productEntity = await _context.Products.FindAsync(Product.Id);
                if (productEntity == null)
                {
                    TempData["ErrorMessage"] = "Товар не найден";
                    return RedirectToPage("./Index");
                }

                if (Product.ImageFile != null && Product.ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(productEntity.ImageUrl) &&
                        !productEntity.ImageUrl.Contains("placeholder") &&
                        !productEntity.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteImage(productEntity.ImageUrl);
                    }

                    var newImageUrl = await _fileService.SaveImageAsync(Product.ImageFile);
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        productEntity.ImageUrl = newImageUrl;
                    }
                }

                productEntity.Name = Product.Name;
                productEntity.Description = Product.Description;
                productEntity.Price = Product.Price;
                productEntity.CategoryId = Product.CategoryId;
                productEntity.IsService = Product.ProductType == "service";
                productEntity.IsActive = Product.IsActive;

                _context.Products.Update(productEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Товар успешно обновлен!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
                await LoadCategories();
                return Page();
            }
        }

        private async Task LoadCategories()
        {
            Product.Categories = await _context.Categories
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}