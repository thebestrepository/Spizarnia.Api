using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;
using Spizarnia.Infrastructure.Persistence;
using Spizarnia.Infrastructure.Persistence.Repositories;
using Spizarnia.Infrastructure.Services;
using Spizarnia.Infrastructure.Wolt;

namespace Spizarnia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMealPlanRepository, MealPlanRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.Configure<WoltOptions>(config.GetSection("Wolt"));
        services.AddMemoryCache();
        services.AddHttpClient<WoltAuthService>();
        services.AddHttpClient<WoltClient>();
        services.AddScoped<IWoltClient, WoltClient>();

        return services;
    }
}
