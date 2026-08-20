using FluentValidation;
using Yellowtail.API.Configuration;
using Yellowtail.API.Contracts.Members;
using Yellowtail.API.Filters;
using Yellowtail.API.Middleware;

namespace Yellowtail.API.Extensions;

/// <summary>
/// Registers ASP.NET Core web-layer services: controllers, validation, Swagger, options, and error handling.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers controllers (with the global <see cref="ValidationFilter"/>), Swagger/OpenAPI generation,
    /// <see cref="PaginationOptions"/> configuration, FluentValidation validators, and the global exception
    /// handler (including <c>AddProblemDetails</c>, which <c>UseExceptionHandler()</c> requires at startup).
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configuration">The configuration to bind <see cref="PaginationOptions"/> from.</param>
    /// <returns>The same service collection instance, for chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>());
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.Configure<PaginationOptions>(configuration.GetSection(PaginationOptions.SectionName));

        services.AddValidatorsFromAssemblyContaining<CreateMemberRequest>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
