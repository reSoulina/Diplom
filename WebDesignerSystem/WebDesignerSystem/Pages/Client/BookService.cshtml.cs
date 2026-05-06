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
    public class BookServiceModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BookServiceModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public Product Service { get; set; }
        public ClientProfile ClientProfile { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Выберите дату")]
            [DataType(DataType.Date)]
            public DateTime AppointmentDate { get; set; }

            [Required(ErrorMessage = "Выберите время")]
            public string AppointmentTime { get; set; }

            [Required(ErrorMessage = "Выберите формат")]
            public string Format { get; set; }

            [Display(Name = "Контактный телефон")]
            public string ContactInfo { get; set; }

            [Display(Name = "Адрес")]
            public string Address { get; set; }

            public string Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Service = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsService && p.IsActive);
            if (Service == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ClientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            }

            // Заполняем форму данными из профиля (если есть)
            Input = new InputModel
            {
                AppointmentDate = DateTime.Now.Date.AddDays(1),
                AppointmentTime = "10:00",
                Format = "",
                ContactInfo = ClientProfile?.Phone ?? "",
                Address = ClientProfile?.DeliveryAddress ?? "",
                Notes = ""
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Service = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsService && p.IsActive);
            if (Service == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!TimeSpan.TryParse(Input.AppointmentTime, out var time))
            {
                ModelState.AddModelError("Input.AppointmentTime", "Неверный формат времени");
                return Page();
            }

            var appointmentDateTime = Input.AppointmentDate.Date.Add(time);

            if (appointmentDateTime <= DateTime.UtcNow.AddHours(1))
            {
                ModelState.AddModelError("Input.AppointmentDate", "Время должно быть в будущем (минимум через час)");
                return Page();
            }

            if (Input.Format == "online" && string.IsNullOrWhiteSpace(Input.ContactInfo))
            {
                ModelState.AddModelError("Input.ContactInfo", "Укажите контактный телефон");
                return Page();
            }

            if (Input.Format == "offline" && string.IsNullOrWhiteSpace(Input.Address))
            {
                ModelState.AddModelError("Input.Address", "Укажите адрес в Уфе");
                return Page();
            }

            var appointment = new ServiceAppointment
            {
                ClientId = user.Id,
                ServiceId = Service.Id,
                AppointmentDateTime = appointmentDateTime,
                DurationMinutes = 60,
                Format = Input.Format,
                ContactInfo = Input.Format == "online" ? Input.ContactInfo : null,
                Address = Input.Format == "offline" ? Input.Address : null,
                Notes = Input.Notes,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _context.ServiceAppointments.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запись создана!";
                return RedirectToPage("/Client/MyAppointments");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.InnerException?.Message ?? ex.Message}");
                return Page();
            }
        }
    }
}