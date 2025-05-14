using Cutify.Domain.Entities;
using Cutify.Services.Interfaces;
using Cutify.Web.Authorization;
using Cutify.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cutify.Web.Controllers;

[Authorize]
public class BarberController : BaseController
{
    private readonly IBarberService _barberService;

    public BarberController(IBarberService barberService, ILogService logService)
        : base(logService)
    {
        _barberService = barberService;
    }

    // Public action - accessible by all authenticated users
    public async Task<IActionResult> Index()
    {
        try
        {
            var barbers = await _barberService.GetAllAsync();
            return View(barbers);
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error retrieving barbers", ex);
            return View(Array.Empty<Barber>());
        }
    }

    // Admin only action
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new BarberViewModel());
    }

    // Admin only action
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BarberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var barber = new Barber
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive
            };

            await _barberService.CreateAsync(barber, model.Password);
            await LogInfoAsync($"New barber created: {barber.Email}");

            TempData["SuccessMessage"] = "Barber created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error creating barber", ex);
            ModelState.AddModelError(string.Empty, "An error occurred while creating the barber.");
            return View(model);
        }
    }

    // Admin only action
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var barber = await _barberService.GetByIdAsync(id);
            if (barber == null)
            {
                return NotFound();
            }

            var model = new BarberViewModel
            {
                Id = barber.Id,
                FirstName = barber.FirstName,
                LastName = barber.LastName,
                Email = barber.Email,
                PhoneNumber = barber.PhoneNumber,
                IsActive = barber.IsActive
            };

            return View(model);
        }
        catch (Exception ex)
        {
            await LogErrorAsync($"Error retrieving barber {id}", ex);
            return RedirectToAction(nameof(Index));
        }
    }

    // Admin only action
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BarberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var barber = await _barberService.GetByIdAsync(model.Id);
            if (barber == null)
            {
                return NotFound();
            }

            barber.FirstName = model.FirstName;
            barber.LastName = model.LastName;
            barber.Email = model.Email;
            barber.PhoneNumber = model.PhoneNumber;
            barber.IsActive = model.IsActive;

            await _barberService.UpdateAsync(barber);
            await LogInfoAsync($"Barber updated: {barber.Email}");

            TempData["SuccessMessage"] = "Barber updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await LogErrorAsync($"Error updating barber {model.Id}", ex);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the barber.");
            return View(model);
        }
    }

    // Admin only action
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _barberService.DeleteAsync(id);
            await LogInfoAsync($"Barber deleted: {id}");

            TempData["SuccessMessage"] = "Barber deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await LogErrorAsync($"Error deleting barber {id}", ex);
            TempData["ErrorMessage"] = "An error occurred while deleting the barber.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Barber only action - can only view their own reservations
    [BarberAuthorization]
    public async Task<IActionResult> MyReservations()
    {
        try
        {
            var barberId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var reservations = await _barberService.GetReservationsAsync(barberId);
            return View(reservations);
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error retrieving barber reservations", ex);
            return View(Array.Empty<Reservation>());
        }
    }
} 