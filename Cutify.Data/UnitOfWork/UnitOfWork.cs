using Cutify.Domain.Entities;
using Cutify.Domain.Interfaces;
using Cutify.Data.Repositories;

namespace Cutify.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly CutifyDbContext _context;
    private IGenericRepository<Barber>? _barbers;
    private IGenericRepository<Reservation>? _reservations;
    private IGenericRepository<Log>? _logs;
    private IGenericRepository<Service>? _services;

    public UnitOfWork(CutifyDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Barber> Barbers => _barbers ??= new GenericRepository<Barber>(_context);
    public IGenericRepository<Reservation> Reservations => _reservations ??= new GenericRepository<Reservation>(_context);
    public IGenericRepository<Log> Logs => _logs ??= new GenericRepository<Log>(_context);
    public IGenericRepository<Service> Services => _services ??= new GenericRepository<Service>(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 