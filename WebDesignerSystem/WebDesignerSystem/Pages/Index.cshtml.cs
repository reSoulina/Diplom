using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public DesignerProfile? DesignerProfile { get; set; }

        public async Task OnGetAsync()
        {
            DesignerProfile = await _context.DesignerProfiles.FirstOrDefaultAsync();
        }
    }
}