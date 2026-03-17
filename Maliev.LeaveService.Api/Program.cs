using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Infrastructure.Repositories;
using Maliev.LeaveService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Program.Log.StartingHost(bootstrapLogger, "Leave Service");

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
    builder.AddStandardCache("leave:"); // Redis + in-memory fallback, memory-optimized

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
    builder.AddStandardCors(); // CORS with fail-fast validation
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
    builder.AddAuthenticatedServiceClient<IEmployeeServiceClient, EmployeeServiceClient>("EmployeeService", sourceServiceName: "leave");

    builder.Services.AddHostedService<Maliev.LeaveService.Infrastructure.BackgroundServices.LeaveAccrualBackgroundService>();
    builder.Services.AddHostedService<Maliev.LeaveService.Infrastructure.BackgroundServices.LeaveExpirationAlertBackgroundService>();
    builder.AddIAMServiceClient("leave");
    builder.Services.AddIAMRegistration<LeaveIAMRegistrationService>("leave");

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- 8. Database Migrations ---
    await app.MigrateDatabaseAsync<LeaveDbContext>();

    // --- 9. Middleware Pipeline ---
    app.UseStandardMiddleware();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    // --- 10. Endpoints ---
    // Map Aspire/Kubernetes endpoints first
    app.MapDefaultEndpoints(servicePrefix: "leave");

    app.MapControllers();
    app.MapApiDocumentation(servicePrefix: "leave");

    Program.Log.ServiceStarted(logger, "Leave Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Program.Log.HostTerminated(bootstrapLogger, ex, "Leave Service");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Entry point for the Leave Service API.
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);
    }
}
