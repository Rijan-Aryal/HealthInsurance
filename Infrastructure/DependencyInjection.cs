using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddHostedService<PolicyLifecycleBackgroundService>();
        services.AddHostedService<ClaimProcessingBackgroundService>();

        return services;
    }
}

