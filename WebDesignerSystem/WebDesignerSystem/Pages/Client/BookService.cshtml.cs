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
        public List<TimeSlot> AvailableTimeSlots { get; set; } = new();

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

        public class TimeSlot
        {
            public string Time { get; set; }
            public string DisplayText { get; set; }
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

            // По умолчанию выбираем завтрашний день (сегодня запрещён, чтобы избежать прошедшего времени)
            var defaultDate = DateTime.Now.Date.AddDays(1);
            // Получаем доступные слоты для этой даты
            AvailableTimeSlots = await GetAvailableTimeSlots(defaultDate);

            Input = new InputModel
            {
                AppointmentDate = defaultDate,
                AppointmentTime = AvailableTimeSlots.FirstOrDefault()?.Time ?? "",
                Format = "",
                ContactInfo = ClientProfile?.Phone ?? "",
                Address = ClientProfile?.DeliveryAddress ?? "",
                Notes = ""
            };

            return Page();
        }

        // AJAX-метод для получения слотов по выбранной дате
        public async Task<IActionResult> OnGetTimeSlots(int serviceId, DateTime date)
        {
            var slots = await GetAvailableTimeSlots(date);
            return new JsonResult(slots.Select(s => new { value = s.Time, text = s.DisplayText }));
        }

        // Вспомогательный метод: генерирует все временные слоты и удаляет занятые
        private async Task<List<TimeSlot>> GetAvailableTimeSlots(DateTime date)
        {
            const int startHour = 9;
            const int endHour = 18;
            const int intervalMinutes = 60;

            var allSlots = new List<TimeSlot>();
            var current = date.Date.AddHours(startHour);
            var end = date.Date.AddHours(endHour);

            while (current < end)
            {
                allSlots.Add(new TimeSlot
                {
                    Time = current.ToString("HH:mm"),
                    DisplayText = current.ToString("HH:mm")
                });
                current = current.AddMinutes(intervalMinutes);
            }

            // Получаем уже занятые слоты (статус не cancelled)
            var busyTimes = await _context.ServiceAppointments
                .Where(a => a.AppointmentDateTime.Date == date && a.Status != "cancelled")
                .Select(a => a.AppointmentDateTime.ToString("HH:mm"))
                .ToListAsync();

            return allSlots.Where(slot => !busyTimes.Contains(slot.Time)).ToList();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Service = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsService && p.IsActive);
            if (Service == null) return NotFound();

            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!TimeSpan.TryParse(Input.AppointmentTime, out var time))
            {
                ModelState.AddModelError("Input.AppointmentTime", "Неверный формат времени");
                // Загружаем доступные слоты для повторного отображения
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
                return Page();
            }

            var appointmentDateTime = Input.AppointmentDate.Date.Add(time);
            if (appointmentDateTime <= DateTime.UtcNow.AddHours(1))
            {
                ModelState.AddModelError("Input.AppointmentDate", "Дата и время должны быть в будущем (минимум через час)");
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
                return Page();
            }

            // Проверка, что выбранное время всё ещё свободно
            var isBusy = await _context.ServiceAppointments
                .AnyAsync(a => a.AppointmentDateTime == appointmentDateTime && a.Status != "cancelled");
            if (isBusy)
            {
                ModelState.AddModelError("Input.AppointmentTime", "Это время уже занято. Выберите другое.");
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
                return Page();
            }

            if (Input.Format == "online" && string.IsNullOrWhiteSpace(Input.ContactInfo))
            {
                ModelState.AddModelError("Input.ContactInfo", "Укажите контактный телефон");
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
                return Page();
            }
            if (Input.Format == "offline" && string.IsNullOrWhiteSpace(Input.Address))
            {
                ModelState.AddModelError("Input.Address", "Укажите адрес в Уфе");
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
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
                TempData["SuccessMessage"] = "Запись успешно создана!";
                return RedirectToPage("/Client/MyAppointments");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.InnerException?.Message ?? ex.Message}");
                AvailableTimeSlots = await GetAvailableTimeSlots(Input.AppointmentDate);
                return Page();
            }
        }
    }
}