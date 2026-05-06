using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Client
{
    [Authorize(Roles = "Client")]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CheckoutModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public OrderInput Input { get; set; } = new();

        public List<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ClientProfile ClientProfile { get; set; }

        public class OrderInput
        {
            [Required(ErrorMessage = "Укажите получателя")]
            [Display(Name = "Получатель")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Укажите телефон")]
            [Phone(ErrorMessage = "Некорректный телефон")]
            [Display(Name = "Телефон")]
            public string Phone { get; set; }

            [Required(ErrorMessage = "Укажите адрес доставки")]
            [Display(Name = "Адрес доставки")]
            public string Address { get; set; }

            [Display(Name = "Комментарий к заказу")]
            public string Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!CartItems.Any()) return RedirectToPage("/Client/Cart/Index");

            TotalAmount = CartItems.Sum(c => c.Product.Price * c.Quantity);

            // Загружаем профиль клиента
            ClientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Подставляем данные из профиля, если они есть
            Input = new OrderInput
            {
                FullName = string.IsNullOrEmpty(ClientProfile?.FullName) ? user.FullName : ClientProfile.FullName,
                Phone = ClientProfile?.Phone ?? "",
                Address = ClientProfile?.DeliveryAddress ?? "",
                Notes = ""
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!CartItems.Any())
            {
                TempData["ErrorMessage"] = "Корзина пуста";
                return RedirectToPage("/Client/Cart/Index");
            }

            if (!ModelState.IsValid)
            {
                TotalAmount = CartItems.Sum(c => c.Product.Price * c.Quantity);
                return Page();
            }

            TotalAmount = CartItems.Sum(c => c.Product.Price * c.Quantity);

            var newOrder = new Order
            {
                ClientId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = TotalAmount,
                CurrentStatusId = 1,
                Notes = Input.Notes,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            foreach (var item in CartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });
            }

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = newOrder.Id,
                StatusId = 1,
                ChangedBy = user.Id,
                Comment = "Заказ создан"
            });

            _context.CartItems.RemoveRange(CartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заказ оформлен!";
            return RedirectToPage("/Client/MyOrders");
        }
    }
}