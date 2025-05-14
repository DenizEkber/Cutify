using Cutify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace Cutify.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CutifyDbContext>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // Check if we already have any barbers
        if (await context.Barbers.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Add admin barber
        var adminBarber = new Barber
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@cutify.com",
            PasswordHash = HashPassword("Admin123!"),
            PhoneNumber = "1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            WorkplaceAddress = "Admin Workplace"
        };

        await context.Barbers.AddAsync(adminBarber);
        await context.SaveChangesAsync();

        // Add sample barbers
        var barbers = new[]
        {
            new Barber
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@cutify.com",
                PasswordHash = HashPassword("Password123!"),
                PhoneNumber = "1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                WorkplaceAddress = "Downtown Barber Shop"
            },
            new Barber
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@cutify.com",
                PasswordHash = HashPassword("Password123!"),
                PhoneNumber = "0987654321",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                WorkplaceAddress = "Uptown Style Studio"
            }
        };

        await context.Barbers.AddRangeAsync(barbers);
        await context.SaveChangesAsync();

        // Seed default services if none exist
        if (!context.Services.Any())
        {
            var services = new List<Service>
            {
                new Service
                {
                    Name = "Haircut",
                    BarberId=4,
                    Description = "Classic haircut with scissors and clippers",
                    DurationMinutes = 30,
                    Price = 25.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "Beard Trim",
                    BarberId=4,
                    Description = "Beard shaping and trimming",
                    DurationMinutes = 20,
                    Price = 15.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "Haircut & Beard Trim",
                    BarberId=4,
                    Description = "Complete grooming package including haircut and beard trim",
                    DurationMinutes = 45,
                    Price = 35.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "Kids Haircut",
                    BarberId=4,
                    Description = "Haircut for children under 12",
                    DurationMinutes = 20,
                    Price = 15.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
} 