using Cutify.Domain.Entities;

namespace Cutify.Services.Interfaces;

public interface IBarberService
{
    Task<Barber?> GetByIdAsync(int id);
    Task<Barber?> GetByEmailAsync(string email);
    Task<IEnumerable<Barber>> GetAllAsync();
    Task<Barber> CreateAsync(Barber barber, string password);
    Task<bool> ValidatePasswordAsync(string email, string password);
    Task UpdateAsync(Barber barber);
    Task DeleteAsync(int id);
    Task<IEnumerable<Reservation>> GetReservationsAsync(int barberId);

    Task<bool> ResetPasswordAsync(string email, string newPassword);

}