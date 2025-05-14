using Cutify.Domain.Entities;

namespace Cutify.Services.Interfaces;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int barberId);
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task UpdateStatusAsync(int id, ReservationStatus status);
    Task DeleteAsync(int id);
    Task<IEnumerable<Reservation>> GetBarberReservationsAsync(int barberId);
    Task<bool> IsTimeSlotAvailableAsync(int barberId, DateTime date, TimeSpan time, int serviceId);

    Task<List<Reservation>> GetByCustomerPhoneAsync(string phone);

    Task<IEnumerable<Reservation>> GetReservationsByDateAsync(int barberId, DateTime date);

}