using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Yellowtail.API.Configuration;
using Yellowtail.API.Filters;
using Yellowtail.API.Middleware;
using Yellowtail.Data;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PaginationOptions>(
    builder.Configuration.GetSection(PaginationOptions.SectionName));

builder.Services.AddDbContext<YellowtailDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ISportRepository, SportRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ISportService, SportService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
