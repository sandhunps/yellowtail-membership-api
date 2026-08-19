using Yellowtail.Services.Contracts;
using Yellowtail.Services.Implementation;

namespace Yellowtail.API.Extensions;

/// <summary>
/// Registers application (business logic) services from <c>Yellowtail.Services</c>.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the member and sport service implementations.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same service collection instance, for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ISportService, SportService>();

        return services;
    }
}
