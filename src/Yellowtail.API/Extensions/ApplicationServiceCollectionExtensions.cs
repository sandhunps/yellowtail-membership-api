using Yellowtail.Services.Configuration;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Implementation;

namespace Yellowtail.API.Extensions;

/// <summary>
/// Registers application (business logic) services from <c>Yellowtail.Services</c>.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the member, sport, and photo-upload service implementations, and binds
    /// <see cref="R2Options"/> from the "R2" configuration section.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configuration">The configuration to bind <see cref="R2Options"/> from.</param>
    /// <returns>The same service collection instance, for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ISportService, SportService>();
        services.AddScoped<IPhotoUploadService, PhotoUploadService>();

        services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));

        return services;
    }
}
