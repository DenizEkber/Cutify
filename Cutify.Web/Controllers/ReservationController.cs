using Cutify.Domain.Entities;
using Cutify.Services.Helper;
using Cutify.Services.Interfaces;
using Cutify.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cutify.Web.Controllers;

[Authorize]
public class ReservationController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IBarberService _barberService;
    private readonly IServiceService _serviceService;
    private readonly ILogService _logService;
    private readonly ISmsService _smsService;

    public ReservationController(
        IReservationService reservationService,
        IBarberService barberService,
        IServiceService serviceService,
        ILogService logService,
        ISmsService smsService)
    {
        _reservationService = reservationService;
        _barberService = barberService;
        _serviceService = serviceService;
        _logService = logService;
        _smsService = smsService;
    }

    [Authorize(Roles = "Barber")]
    public async Task<IActionResult> List()
    {
        var barberId = int.Parse(User.FindFirst("BarberId")?.Value ?? "0");
        var reservations = await _reservationService.GetBarberReservationsAsync(barberId);
        return View(reservations);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    [HttpPost]
    [Authorize(Roles = "Barber")]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
    {
        try
        {
            await _reservationService.UpdateStatusAsync(id, status);
            TempData["SuccessMessage"] = "Reservation status updated successfully.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error updating reservation status", ex);
            TempData["ErrorMessage"] = "An error occurred while updating the reservation status.";
        }

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    [Authorize(Roles = "Barber")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _reservationService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Reservation deleted successfully.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error deleting reservation", ex);
            TempData["ErrorMessage"] = "An error occurred while deleting the reservation.";
        }

        return RedirectToAction(nameof(List));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search)
    {
        var barbers = await _barberService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            barbers = barbers
                .Where(b => b.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return View(barbers);
    }


    [AllowAnonymous]
    public async Task<IActionResult> Create(int barberId)
    {
        var services = await _serviceService.GetByBarberIdAsync(barberId);
        var barbers = await _barberService.GetByIdAsync(barberId);

        var viewModel = new ReservationViewModel
        {
            BarberId = barberId,
            BarberName = barbers.FullName,
            AvailableServices = services.Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Description = s.Description
            }).ToList(),
            AvailableTimes = await GenerateTimeSlotsAsync(barberId, DateTime.Today)
        };

        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableTimes(int barberId, DateTime date)
    {
        var times = await GenerateTimeSlotsAsync(barberId, date);
        return Json(times);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(PublicReservationViewModel viewModel)
    {
        ModelState.Remove("BarberName");

        if (!ModelState.IsValid)
        {
            viewModel.Barbers = await _barberService.GetAllAsync();
            viewModel.Services = await _serviceService.GetAllAsync();
            viewModel.AvailableTimes = await GenerateTimeSlotsAsync(viewModel.BarberId, viewModel.Date);
            return View("Index", viewModel);
        }

        try
        {
            // Check if the time slot is available
            var isAvailable = await _reservationService.IsTimeSlotAvailableAsync(
                viewModel.BarberId,
                viewModel.Date,
                viewModel.Time,
                viewModel.ServiceId);

            if (!isAvailable)
            {
                ModelState.AddModelError("", "The selected time slot is not available.");
                viewModel.Barbers = await _barberService.GetAllAsync();
                viewModel.Services = await _serviceService.GetAllAsync();
                viewModel.AvailableTimes = await GenerateTimeSlotsAsync(viewModel.BarberId, viewModel.Date);

                return View("Create", viewModel);
            }

            var reservation = new Reservation
            {
                BarberId = viewModel.BarberId,
                ServiceId = viewModel.ServiceId,
                CustomerName = viewModel.CustomerName,
                CustomerPhone = viewModel.CustomerPhone,
                Date = viewModel.Date,
                Time = viewModel.Time,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _reservationService.CreateAsync(reservation);
            TempData["SuccessMessage"] = "Reservation created successfully.";

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(365),
                IsEssential = true, // GDPR üçün vacibdirs?
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("CustomerName", viewModel.CustomerName, cookieOptions);
            Response.Cookies.Append("CustomerPhone", viewModel.CustomerPhone, cookieOptions);


            return View("ReservationSuccess");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error creating reservation", ex);
            ModelState.AddModelError("", "An error occurred while creating the reservation.");
            viewModel.Barbers = await _barberService.GetAllAsync();
            viewModel.Services = await _serviceService.GetAllAsync();
            return View("Create", viewModel);
        }
    }




    [HttpGet]
    [AllowAnonymous]
    public IActionResult RequestVerification()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RequestVerification(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            ModelState.AddModelError("", "Please enter your phone number.");
            return View();
        }

        var code = VerificationHelper.GenerateCode();

        // Kod Session-a yaz?l?r
        HttpContext.Session.SetString("VerifyPhone", phone);
        HttpContext.Session.SetString("VerifyCode", code);
        HttpContext.Session.SetString("VerifyCodeTime", DateTime.UtcNow.ToString());

        await _smsService.SendVerificationCodeAsync(phone, code);

        return RedirectToAction("VerifyCode");
    }





    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyCode()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCode(string inputCode)
    {
        var storedCode = HttpContext.Session.GetString("VerifyCode");
        var storedPhone = HttpContext.Session.GetString("VerifyPhone");
        var codeTimeStr = HttpContext.Session.GetString("VerifyCodeTime");

        if (storedCode == null || storedPhone == null || codeTimeStr == null)
        {
            TempData["ErrorMessage"] = "Session expired. Please try again.";
            return RedirectToAction("RequestVerification");
        }

        if (DateTime.TryParse(codeTimeStr, out var codeTime))
        {
            if ((DateTime.UtcNow - codeTime).TotalMinutes > 5)
            {
                TempData["ErrorMessage"] = "Verification code expired. Please request a new one.";
                return RedirectToAction("RequestVerification");
            }
        }

        if (inputCode != storedCode)
        {
            ModelState.AddModelError("", "Incorrect verification code.");
            return View();
        }

        // Do?rulama keçdi: rezervasiyalar göst?rilir
        var reservations = await _reservationService.GetByCustomerPhoneAsync(storedPhone);
        HttpContext.Session.Remove("VerifyCode");
        HttpContext.Session.Remove("VerifyPhone");

        return View("MyReservationsList", reservations);
    }


    private async Task<List<SelectListItem>> GenerateTimeSlotsAsync(int barberId, DateTime date)
    {
        var list = new List<SelectListItem>();
        var startTime = new TimeSpan(9, 0, 0); // 09:00
        var endTime = new TimeSpan(18, 0, 0); // 18:00

        // Bugünkü günse, geçmiş saatleri filtrele
        if (date.Date == DateTime.Now.Date)
        {
            var now = DateTime.Now.TimeOfDay;
            // 30 dəqiqəlik ən yaxın intervala yuvarlaqla (məs: 16:55 → 17:00)
            startTime = RoundUpToNextHalfHour(now);
        }

        var reservations = await _reservationService.GetReservationsByDateAsync(barberId, date);
        var reservedTimes = reservations.Select(r => r.Time).ToHashSet();

        while (startTime < endTime)
        {
            if (!reservedTimes.Contains(startTime))
            {
                list.Add(new SelectListItem
                {
                    Value = startTime.ToString(@"hh\:mm"),
                    Text = startTime.ToString(@"hh\:mm")
                });
            }
            startTime = startTime.Add(TimeSpan.FromMinutes(30));
        }

        return list;
    }

    private TimeSpan RoundUpToNextHalfHour(TimeSpan time)
    {
        var totalMinutes = (int)Math.Ceiling(time.TotalMinutes / 30.0) * 30;
        return TimeSpan.FromMinutes(totalMinutes);
    }


}