using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Admin.Categories
{
    [Authorize(Roles = "Designer")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Category Category { get; set; }
        public int ProductCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                TempData["ErrorMessage"] = "Категория не найдена";
                return RedirectToPage("./Index");
            }

            // Проверяем, есть ли товары в этой категории
            ProductCount = await _context.Products
                .Where(p => p.CategoryId == id)
                .CountAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Категория не найдена";
                    return RedirectToPage("./Index");
                }

                // Проверяем, есть ли товары в этой категории
                var productCount = await _context.Products
                    .Where(p => p.CategoryId == id)
                    .CountAsync();

                if (productCount > 0)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить категорию, в которой есть товары. Сначала переместите или удалите товары.";
                    return RedirectToPage("./Index");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Категория '{category.Name}' успешно удалена!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}