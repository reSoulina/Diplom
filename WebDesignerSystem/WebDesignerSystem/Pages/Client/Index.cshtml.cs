using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebDesignerSystem.Pages.Client
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}