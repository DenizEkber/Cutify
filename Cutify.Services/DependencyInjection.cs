using Cutify.Services.Implementations;
using Cutify.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cutify.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IBarberService, BarberService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ILogService, LogService>();

        return services;
    }
} 