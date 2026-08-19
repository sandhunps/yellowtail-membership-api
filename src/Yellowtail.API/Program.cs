using Serilog;
using Yellowtail.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplicationServices()
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseYellowtailPipeline();

app.Run();
