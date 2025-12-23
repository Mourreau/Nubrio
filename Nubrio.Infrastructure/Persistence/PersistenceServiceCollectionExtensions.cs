using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nubrio.Application.Common;
using Nubrio.Application.Interfaces.Repository;
using Nubrio.Infrastructure.Persistence.Repositories;
using Nubrio.Infrastructure.Telemetry;

namespace Nubrio.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var nubrioConnectionString = configuration.GetConnectionString("NubrioDb");
        if  (string.IsNullOrWhiteSpace(nubrioConnectionString)) 
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:NubrioDb");
        
        services.AddDbContext<NubrioDbContext>(options =>
        {
            options.UseNpgsql(
                nubrioConnectionString,
                npgsqlOptions => { npgsqlOptions.MigrationsAssembly(typeof(NubrioDbContext).Assembly.FullName); });

            options.UseSnakeCaseNamingConvention();
        });

        services.AddMemoryCache();
        services.AddScoped<IRequestLogStore, EfRequestLogStore>();
        services.AddScoped<IStatsRepository, StatsRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        

        return services;
    }
}