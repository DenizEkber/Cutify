using Cutify.Domain.Entities;
using Cutify.Services.Implementations;
using Cutify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cutify.Web.Controllers;

[Authorize]
public class ServiceController : Controller
{
    private readonly IServiceService _serviceService;
    private readonly IBarberService _barberService;
    private readonly ILogService _logService;

    public ServiceController(IServiceService serviceService, ILogService logService, IBarberService barberService)
    {
        _serviceService = serviceService;
        _logService = logService;
        _barberService = barberService;
    }

    public async Task<IActionResult> Index()
    {
        var username = User.Identity!.Name!;
        var barber = await _barberService.GetByEmailAsync(username);
        if (barber == null)
        {
            return Unauthorized();
        }

        var services = await _serviceService.GetByBarberIdAsync(barber.Id);
        return View(services);
    }


    public IActionResult Create()
    {
        return View();
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Create(Service service)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return View(service);
    //    }

    //    try
    //    {
    //        await _serviceService.CreateAsync(service);
    //        TempData["SuccessMessage"] = "Service created successfully.";
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch (Exception ex)
    //    {
    //        await _logService.LogErrorAsync("Error creating service", ex);
    //        ModelState.AddModelError("", "An error occurred while creating the service.");
    //        return View(service);
    //    }
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service service)
    {
        ModelState.Remove("Barber");

        if (!ModelState.IsValid)
            return View(service);

        try
        {
            // Barber ID-ni ?ld? et
            var username = User.Identity!.Name!;
            var barber = await _barberService.GetByEmailAsync(username);
            if (barber == null)
            {
                return Unauthorized();
            }

            service.BarberId = barber.Id;
            await _serviceService.CreateAsync(service);

            TempData["SuccessMessage"] = "Service created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error creating service", ex);
            ModelState.AddModelError("", "An error occurred while creating the service.");
            return View(service);
        }
    }



    public async Task<IActionResult> Edit(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Service service)
    {
        if (id != service.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(service);
        }

        try
        {
            // Get the existing service to preserve the BarberId
            var existingService = await _serviceService.GetByIdAsync(id);
            if (existingService == null)
            {
                return NotFound();
            }

            // Preserve the BarberId
            service.BarberId = existingService.BarberId;
            
            await _serviceService.UpdateAsync(service);
            TempData["SuccessMessage"] = "Service updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error updating service", ex);
            ModelState.AddModelError("", "An error occurred while updating the service.");
            return View(service);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _serviceService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Service deleted successfully.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Error deleting service", ex);
            TempData["ErrorMessage"] = "An error occurred while deleting the service.";
        }

        return RedirectToAction(nameof(Index));
    }
} 