using Cutify.Data;
using Cutify.Domain.Entities;
using Cutify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cutify.Services.Implementations;

public class ServiceService : IServiceService
{
    private readonly CutifyDbContext _context;
    private readonly ILogService _logService;

    public ServiceService(CutifyDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services.FindAsync(id);
    }

    public async Task<IEnumerable<Service>> GetByBarberIdAsync(int barberId)
    {
        return await _context.Services
            .Where(s => s.BarberId == barberId)
            .ToListAsync();
    }


    public async Task<Service> CreateAsync(Service service)
    {
        service.IsActive = true;
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Service created: {service.Name}");
        return service;
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Service updated: {service.Name}");
    }

    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            service.IsActive = false;
            await _context.SaveChangesAsync();
            await _logService.LogAsync($"Service deleted: {service.Name}");
        }
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int barberId, DateTime date, TimeSpan time, int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null) return false;

        var endTime = time.Add(TimeSpan.FromMinutes(service.DurationMinutes));

        var conflictingReservations = await _context.Reservations
            .Where(r => r.BarberId == barberId && 
                       r.Date == date &&
                       r.Status != ReservationStatus.Cancelled &&
                       ((r.Time <= time && r.Time.Add(TimeSpan.FromMinutes(r.Service!.DurationMinutes)) > time) ||
                        (r.Time < endTime && r.Time.Add(TimeSpan.FromMinutes(r.Service!.DurationMinutes)) >= endTime)))
            .AnyAsync();

        return !conflictingReservations;
    }
} 