using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cutify.Web.Models;
using Cutify.Domain.Entities;
using Cutify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Cutify.Web.Controllers;

[Authorize]
public class HomeController : BaseController
{
    private readonly IReservationService _reservationService;
    private readonly IBarberService _barberService;

    public HomeController(
        IReservationService reservationService,
        IBarberService barberService,
        ILogService logService)
        : base(logService)
    {
        _reservationService = reservationService;
        _barberService = barberService;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
       if (User.Identity.IsAuthenticated)
        {
            // ?stifad?çi login olubsa, yönl?ndir (m?s?l?n, Dashboard s?hif?sin?)
            return RedirectToAction("Dashboard", "Home"); // Misal üçün
        }

        return View();
    }

    //[Authorize(Roles = "Admin")]
    /*public async Task<IActionResult> Dashboard()
    {
        try
        {
            var barberId = int.Parse(User.FindFirst("BarberId")?.Value ?? "0");
            var reservations = await _reservationService.GetUpcomingReservationsAsync(barberId);
            return View(reservations);
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error loading dashboard", ex);
            return View(Enumerable.Empty<Reservation>());
        }
    }*/


    public async Task<IActionResult> Dashboard(DateTime? searchDate)
    {
        try
        {
            var barberId = int.Parse(User.FindFirst("BarberId")?.Value ?? "0");

            IEnumerable<Reservation> reservations;

            if (searchDate.HasValue)
            {
                ViewBag.SearchDate = searchDate.Value.ToString("yyyy-MM-dd"); 
                reservations = await _reservationService.GetReservationsByDateAsync(barberId, searchDate.Value);
            }
            else
            {
                ViewBag.SearchDate = "";
                reservations = await _reservationService.GetUpcomingReservationsAsync(barberId);
            }



            return View(reservations);
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error loading dashboard", ex);
            return View(Enumerable.Empty<Reservation>());
        }
    }
    


    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
