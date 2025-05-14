using System.Security.Cryptography;
using System.Text;
using Cutify.Data;
using Cutify.Domain.Entities;
using Cutify.Domain.Interfaces;
using Cutify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Cutify.Services.Implementations;

public class BarberService : IBarberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CutifyDbContext _context;
    private readonly ILogService _logService;

    public BarberService(IUnitOfWork unitOfWork, CutifyDbContext context, ILogService logService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logService = logService;
    }

    public async Task<Barber?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Barbers.GetByIdAsync(id);
    }

    public async Task<Barber?> GetByEmailAsync(string email)
    {
        var barbers = await _unitOfWork.Barbers.FindAsync(b => b.Email == email);
        return barbers.FirstOrDefault();
    }


    public async Task<IEnumerable<Barber>> GetAllAsync()
    {
        return await _context.Barbers
            .Where(b => b.IsActive)
            .OrderBy(b => b.LastName)
            .ThenBy(b => b.FirstName)
            .ToListAsync();
    }

    public async Task<Barber> CreateAsync(Barber barber, string password)
    {
        barber.PasswordHash = HashPassword(password);
        barber.CreatedAt = DateTime.UtcNow;
        barber.IsActive = true;

        await _unitOfWork.Barbers.AddAsync(barber);
        await _unitOfWork.CompleteAsync();
        await _logService.LogAsync($"Barber created: {barber.Email}");
        return barber;
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var barber = await GetByEmailAsync(email);
        if (barber == null) return false;

        var hashedPassword = HashPassword(password);
        return barber.PasswordHash == hashedPassword;
    }

    public async Task UpdateAsync(Barber barber)
    {
        var existingBarber = await _context.Barbers.FindAsync(barber.Id);
        if (existingBarber == null)
        {
            throw new KeyNotFoundException($"Barber with ID {barber.Id} not found.");
        }

        // Only update password if it's being changed
        if (!string.IsNullOrEmpty(barber.PasswordHash) && barber.PasswordHash != existingBarber.PasswordHash)
        {
            barber.PasswordHash = HashPassword(barber.PasswordHash);
        }
        else
        {
            barber.PasswordHash = existingBarber.PasswordHash;
        }

        _context.Entry(existingBarber).CurrentValues.SetValues(barber);
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Barber updated: {barber.Email}");
    }

    public async Task DeleteAsync(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null)
        {
            throw new KeyNotFoundException($"Barber with ID {id} not found.");
        }

        barber.IsActive = false;
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Barber deactivated: {barber.Email}");
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(int barberId)
    {
        return await _context.Reservations
            .Include(r => r.Service)
            .Where(r => r.BarberId == barberId)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Time)
            .ToListAsync();
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var barber = await GetByEmailAsync(email);
        if (barber == null)
        {
            return false; // ?stifad?çi tap?lmad?
        }

        barber.PasswordHash = HashPassword(newPassword);
        _context.Barbers.Update(barber);
        await _context.SaveChangesAsync();
        await _logService.LogAsync($"Password reset for: {barber.Email}");

        return true;
    }



    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
} 