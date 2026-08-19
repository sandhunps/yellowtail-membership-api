using Microsoft.EntityFrameworkCore;
using Yellowtail.Data;
using Yellowtail.Data.Repositories;

namespace Yellowtail.API.Extensions;

/// <summary>
/// Registers persistence-layer services: the EF Core <see cref="YellowtailDbContext"/> and repositories.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="YellowtailDbContext"/> (configured for PostgreSQL via the "Default" connection
    /// string) and the repository implementations for members and sports.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configuration">The configuration to read the connection string from.</param>
    /// <returns>The same service collection instance, for chaining.</returns>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<YellowtailDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ISportRepository, SportRepository>();

        return services;
    }
}
