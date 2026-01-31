using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Поле обязательно")]
            [Display(Name = "Полное имя")]
            [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Некорректный email адрес")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Пароль обязателен")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Подтвердите пароль")]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Проверяем, есть ли уже пользователь с таким email
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким email уже существует");
                    return Page();
                }

                // Находим роль Client в нашей таблице Roles
                var clientRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Client");

                if (clientRole == null)
                {
                    // Если роли нет, создаем ее
                    clientRole = new Role { Name = "Client", Description = "Роль клиента" };
                    _context.Roles.Add(clientRole);
                    await _context.SaveChangesAsync();
                }

                // Создаем нового пользователя
                var user = new User
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    RoleId = clientRole.Id,  // Связываем с нашей таблицей ролей
                    EmailConfirmed = true,   // Для тестирования подтверждаем сразу
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Добавляем пользователю роль "Client" в Identity
                    await _userManager.AddToRoleAsync(user, "Client");

                    _logger.LogInformation("Пользователь создал новую учетную запись с паролем.");

                    // Автоматически входим после регистрации
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(ReturnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}