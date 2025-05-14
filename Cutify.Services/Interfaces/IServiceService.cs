using Cutify.Domain.Entities;

namespace Cutify.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<Service>> GetAllAsync();
    Task<Service?> GetByIdAsync(int id);
    Task<Service> CreateAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int id);
    Task<bool> IsTimeSlotAvailableAsync(int barberId, DateTime date, TimeSpan time, int serviceId);

    Task<IEnumerable<Service>> GetByBarberIdAsync(int barberId);

}