using Microsoft.Extensions.Hosting;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.Repositories;
using Maliev.LeaveService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume();

// --- 2. Infrastructure & Observability ---
builder.AddServiceDefaults();
builder.AddStandardMiddleware(options =>
{
    options.EnableRequestLogging = true;
});
builder.AddServiceMeters("leave-service");

// --- 3. Data & Cache ---
builder.AddPostgresDbContext<LeaveDbContext>(connectionName: "LeaveDbContext");
builder.AddRedisDistributedCache(instanceName: "leave:");

// --- 4. Messaging ---
builder.AddMassTransitWithRabbitMq(x =>
{
    x.AddConsumer<Maliev.LeaveService.Infrastructure.Consumers.EmployeeCreatedEventConsumer>();
    x.AddConsumer<Maliev.LeaveService.Infrastructure.Consumers.EmployeeTerminatedEventConsumer>();
    x.AddConsumer<Maliev.LeaveService.Infrastructure.Consumers.UndoCloseLeaveBalanceConsumer>();
});

// --- 5. Security ---
builder.AddJwtAuthentication();

// --- 6. API Configuration ---
builder.AddDefaultCors();
builder.AddDefaultApiVersioning();
builder.AddStandardRateLimiting();

if (!builder.Environment.IsProduction())
{
    builder.AddStandardOpenApi(
        title: "MALIEV Leave Service API",
        description: "Manages employee leave requests, balances, and policies.");
}

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
});

// --- 7. Application Services ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Maliev.LeaveService.Application.Commands.SubmitLeaveRequestCommand).Assembly));

builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ILeavePolicyRepository, LeavePolicyRepository>();
builder.Services.AddScoped<ILeaveApprovalRepository, LeaveApprovalRepository>();
builder.Services.AddScoped<Maliev.LeaveService.Application.Commands.Handlers.UndoCloseLeaveBalanceCommandHandler>();

builder.AddServiceClient<INotificationService, NotificationService>("NotificationService");
builder.AddServiceClient<EmployeeServiceClient>("EmployeeService");

builder.Services.AddHostedService<Maliev.LeaveService.Infrastructure.BackgroundServices.LeaveAccrualBackgroundService>();
builder.Services.AddHostedService<Maliev.LeaveService.Infrastructure.BackgroundServices.LeaveExpirationAlertBackgroundService>();
builder.Services.AddIAMRegistration<LeaveIAMRegistrationService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// --- 8. Database Migrations ---
try
{
    await app.MigrateDatabaseAsync<LeaveDbContext>();
}
catch (Exception ex)
{
    logger.LogError(ex, "Database migration failed");
}

// --- 9. Middleware Pipeline ---
app.UseStandardMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// --- 10. Endpoints ---
app.MapControllers();
app.MapDefaultEndpoints(servicePrefix: "leave");
app.MapApiDocumentation(servicePrefix: "leave");

await app.RunAsync();

public partial class Program { }
