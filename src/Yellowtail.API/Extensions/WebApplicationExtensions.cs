using Serilog;

namespace Yellowtail.API.Extensions;

/// <summary>
/// Configures the ASP.NET Core HTTP request pipeline for the Yellowtail API.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Wires up request logging, Swagger UI (development only), the global exception handler,
    /// HTTPS redirection, and controller routing, in that order.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The same application instance, for chaining.</returns>
    public static WebApplication UseYellowtailPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.MapControllers();

        return app;
    }
}
