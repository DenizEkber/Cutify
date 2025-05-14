using Cutify.Data;
using Cutify.Domain.Entities;
using Cutify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cutify.Services.Implementations;

public class ReservationService : IReservationService
{
    private readonly CutifyDbContext _context;
    private readonly ILogService _logService;

    public ReservationService(CutifyDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int barberId)
    {
        return await _context.Reservations
        .Where(r => r.BarberId == barberId /*&& r.Date >= DateTime.UtcNow*/) // sadece gelecekteki rezervasyonlar
        .Include(r => r.Service) // Servis bilgilerini de dahil et
        .OrderBy(r => r.Date)
        .ToListAsync();
    }


    public async Task<IEnumerable<Reservation>> GetReservationsByDateAsync(int barberId, DateTime date)
    {
        return await _context.Reservations
            .Where(r => r.BarberId == barberId && r.Date.Date == date.Date)
            .Include(r => r.Service)
            .ToListAsync();
    }


    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        reservation.Status = ReservationStatus.Pending;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Reservation created: {reservation.CustomerName} for {reservation.Date:d} at {reservation.Time:t}");
        return reservation;
    }

    public async Task UpdateStatusAsync(int id, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation != null)
        {
            reservation.Status = status;
            await _context.SaveChangesAsync();
            await _logService.LogAsync($"Reservation {id} status updated to {status}");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            await _logService.LogAsync($"Reservation {id} deleted");
        }
    }

    public async Task<IEnumerable<Reservation>> GetBarberReservationsAsync(int barberId)
    {
        return await _context.Reservations
            .Include(r => r.Service)
            .Where(r => r.BarberId == barberId)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Time)
            .ToListAsync();
    }

    //public async Task<bool> IsTimeSlotAvailableAsync(int barberId, DateTime date, TimeSpan time, int serviceId)
    //{
    //    var service = await _context.Services.FindAsync(serviceId);
    //    if (service == null) return false;

    //    var startTime = time;
    //    var endTime = time + TimeSpan.FromMinutes(service.DurationMinutes);


    //    var overlappingReservation = await _context.Reservations
    //    .Where(r => r.BarberId == barberId && r.Date == date)
    //    .Join(_context.Services,
    //        reservation => reservation.ServiceId,
    //        svc => svc.Id,
    //        (reservation, svc) => new { Reservation = reservation, Service = svc })
    //    .Where(rs =>
    //        rs.Reservation.Time < endTime &&
    //        rs.Reservation.Time + TimeSpan.FromMinutes(rs.Service.DurationMinutes) > startTime
    //    )
    //    .FirstOrDefaultAsync();

    //    return overlappingReservation == null;
    //}
    public async Task<bool> IsTimeSlotAvailableAsync(int barberId, DateTime date, TimeSpan time, int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null) return false;

        var startTime = time;
        var endTime = time + TimeSpan.FromMinutes(service.DurationMinutes);

        // Client tərəfdə işləmək üçün AsEnumerable() istifadə edirik
        var reservations = await _context.Reservations
            .Where(r => r.BarberId == barberId && r.Date == date)
            .Include(r => r.Service)
            .ToListAsync();

        var overlappingReservation = reservations.FirstOrDefault(r =>
            r.Time < endTime &&
            r.Time + TimeSpan.FromMinutes(r.Service.DurationMinutes) > startTime);

        return overlappingReservation == null;

    }




    public async Task<List<Reservation>> GetByCustomerPhoneAsync(string phone)
    {
        return await _context.Reservations
            .Where(r => r.CustomerPhone == phone)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Time)
            .ToListAsync();
    }

}