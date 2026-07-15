using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OsintToolkit.Core.Interfaces;
using OsintToolkit.Core.Services;
using OsintToolkit.Infrastructure.Data;
using OsintToolkit.Infrastructure.Repositories;

namespace OsintToolkit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["UseInMemoryDatabase"] == "true")
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("OsintToolkit_InMemory"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        }

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Register repositories and services
        services.AddScoped<IScanRepository, ScanRepository>();
        services.AddScoped<IScanService, ScanService>();

        return services;
    }
}


