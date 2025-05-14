using Cutify.Domain.Entities;

namespace Cutify.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Barber> Barbers { get; }
    IGenericRepository<Reservation> Reservations { get; }
    IGenericRepository<Log> Logs { get; }
    IGenericRepository<Service> Services { get; }
    Task<int> CompleteAsync();
} 