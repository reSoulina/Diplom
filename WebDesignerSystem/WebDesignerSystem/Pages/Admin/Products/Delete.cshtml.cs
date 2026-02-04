using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Services;

namespace WebDesignerSystem.Pages.Admin.Products
{
    [Authorize(Roles = "Designer")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ApplicationDbContext context, IFileService fileService, ILogger<DeleteModel> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        [BindProperty]
        public Models.Entities.Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null)
            {
                TempData["ErrorMessage"] = "Товар не найден";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Товар не найден";
                    return RedirectToPage("./Index");
                }

                // Удаляем изображение, если оно есть
                if (!string.IsNullOrEmpty(product.ImageUrl) &&
                    !product.ImageUrl.Contains("placeholder") &&
                    !product.ImageUrl.StartsWith("http"))
                {
                    _fileService.DeleteImage(product.ImageUrl);
                }

                // Удаляем товар
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Товар {id} удален");
                TempData["SuccessMessage"] = $"Товар '{product.Name}' успешно удален!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении товара");
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}