using BirthdayReminder.API.Infrastructure;
using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Application.Services;
using BirthdayReminder.Infrastructure.Persistences;
using BirthdayReminder.Infrastructure.Repositories;
using BirthdayReminder.Infrastructure.Services;
using BirthdayReminder.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(option => option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<TimeZoneService>();
builder.Services.AddHttpClient<IEmailReminder, EmailReminder>(client =>
{
    var emailApiBaseUrl = builder.Configuration["ExternalServices:EmailApi:BaseUrl"]
        ?? throw new InvalidOperationException("ExternalServices:EmailApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(emailApiBaseUrl.EndsWith('/') ? emailApiBaseUrl : emailApiBaseUrl + "/");
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IReminderStrategy, BirthdayReminderStrategy>();
builder.Services.AddHostedService<ReminderWorker>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
