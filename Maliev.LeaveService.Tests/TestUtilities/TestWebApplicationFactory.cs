using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using Moq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using MassTransit;

namespace Maliev.LeaveService.Tests.TestUtilities;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = 
#pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine").Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder().WithImage("redis:8.4-alpine").Build();
    private readonly RabbitMqContainer _rabbitmqContainer = new RabbitMqBuilder().WithImage("rabbitmq:4.2-alpine").Build();
#pragma warning restore CS0618
    private readonly RSA _testRsa = RSA.Create(2048);

    public TestWebApplicationFactory()
    {
        // Set environment variables EARLY so Program.cs picks them up
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("CORS__AllowedOrigins__0", "http://localhost:3000");
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", "http://localhost:3000");
    }

    public string CreateTestToken(string userId = "test-user", string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new RsaSecurityKey(_testRsa);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("CORS:AllowedOrigins:0", "http://localhost:3000");
        builder.UseSetting("Features:FailOpenOnIAMError", "true");

        // Export RSA public key for JWT validation in PEM format
        var publicKeyPem = _testRsa.ExportRSAPublicKeyPem();
        var publicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyPem));
        Environment.SetEnvironmentVariable("Jwt__PublicKey", publicKeyBase64);
        Environment.SetEnvironmentVariable("Jwt:PublicKey", publicKeyBase64);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:NotificationService:BaseUrl"] = "http://test-notification",
                ["Services:EmployeeService:BaseUrl"] = "http://test-employee",
                ["IAM:RegistrationDelaySeconds"] = "0",
                ["RateLimiting:PermitLimit"] = "10000",
                ["RateLimiting:WindowMinutes"] = "1"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "test-issuer",
                    ValidateAudience = true,
                    ValidAudience = "test-audience",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(_testRsa)
                };
            });

            // Mock IAM service client - return true for all permission checks to allow tests to pass
            services.AddScoped<Maliev.Aspire.ServiceDefaults.IAM.IIamServiceClient>(sp => {
                var mockIam = new Moq.Mock<Maliev.Aspire.ServiceDefaults.IAM.IIamServiceClient>();
                mockIam.Setup(x => x.CheckPermissionAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string?>(), Moq.It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
                return mockIam.Object;
            });

            services.AddMassTransitTestHarness();
        });
    }

    [SuppressMessage("Security", "EF1002:Gaps in SQL queries", Justification = "Table names are known constants and are safe.")]
    public async Task ResetDatabaseAsync()
    {
        // Create a new DbContext directly with the test container connection string
        var connectionString = _postgresContainer.GetConnectionString();
        Console.WriteLine($"Using connection string: {connectionString}");

        var options = new DbContextOptionsBuilder<LeaveDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new LeaveDbContext(options);

        Console.WriteLine($"Using connection string: {connectionString}");

        // Create tables using raw SQL - each statement separately for reliability
        // Note: xmin is a shadow property in EF Core, don't include it in raw SQL
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""accrual_runs"" (
                ""id"" uuid PRIMARY KEY,
                ""year"" int NOT NULL,
                ""month"" int NOT NULL,
                ""run_at"" timestamp with time zone NOT NULL,
                ""employees_processed"" int NOT NULL,
                ""is_success"" bool NOT NULL
            )");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""leave_balances"" (
                ""id"" uuid PRIMARY KEY,
                ""employee_id"" uuid NOT NULL,
                ""leave_type"" int NOT NULL,
                ""year"" int NOT NULL,
                ""entitled"" numeric(5,2) NOT NULL,
                ""used"" numeric(5,2) NOT NULL,
                ""pending"" numeric(5,2) NOT NULL,
                ""carried_forward"" numeric(5,2) NOT NULL,
                ""expiration_date"" timestamp with time zone
            )");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""leave_policies"" (
                ""id"" uuid PRIMARY KEY,
                ""leave_type"" int NOT NULL,
                ""default_entitlement"" numeric(5,2) NOT NULL,
                ""accrual_rate"" numeric(5,2) NOT NULL,
                ""max_carry_forward"" numeric(5,2) NOT NULL,
                ""max_consecutive_days"" int NOT NULL,
                ""required_approval_levels"" int NOT NULL,
                ""is_active"" bool NOT NULL
            )");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""leave_requests"" (
                ""id"" uuid PRIMARY KEY,
                ""employee_id"" uuid NOT NULL,
                ""leave_type"" int NOT NULL,
                ""start_date"" timestamp with time zone NOT NULL,
                ""end_date"" timestamp with time zone NOT NULL,
                ""total_days"" numeric(5,2) NOT NULL,
                ""half_day_period"" int NOT NULL,
                ""reason"" text,
                ""status"" int NOT NULL,
                ""created_at"" timestamp with time zone NOT NULL,
                ""updated_at"" timestamp with time zone
            )");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""leave_approvals"" (
                ""id"" uuid PRIMARY KEY,
                ""leave_request_id"" uuid NOT NULL,
                ""approver_id"" uuid NOT NULL,
                ""status"" int NOT NULL,
                ""comments"" text,
                ""decided_at"" timestamp with time zone NOT NULL,
                FOREIGN KEY (""leave_request_id"") REFERENCES ""leave_requests""(""id"") ON DELETE CASCADE
            )");

        // Create indexes
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_requests_employee_id\" ON \"leave_requests\"(\"employee_id\")");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_requests_status\" ON \"leave_requests\"(\"status\")");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_approvals_approver_id\" ON \"leave_approvals\"(\"approver_id\")");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_approvals_leave_request_id\" ON \"leave_approvals\"(\"leave_request_id\")");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_balances_employee_id_leave_type_year\" ON \"leave_balances\"(\"employee_id\", \"leave_type\", \"year\")");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"ix_leave_policies_leave_type\" ON \"leave_policies\"(\"leave_type\")");
        await context.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS \"ix_accrual_runs_year_month\" ON \"accrual_runs\"(\"year\", \"month\")");

        // Now get the context from the service provider for truncating and seeding
        using var scope = Services.CreateScope();
        var serviceContext = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        // Truncate all tables (ignore errors if tables don't exist - first run scenario)
        var tableNames = new[] { "leave_approvals", "leave_requests", "leave_balances", "leave_policies", "accrual_runs" };
        foreach (var table in tableNames)
        {
            try
            {
                await serviceContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE");
            }
            catch
            {
                // Ignore - table might not exist on first run
            }
        }

        // Seed default leave policies
        await SeedDefaultPoliciesAsync(serviceContext);
    }

    private async Task SeedDefaultPoliciesAsync(LeaveDbContext context)
    {
        // Seed common leave policies for tests
        if (!await context.LeavePolicies.AnyAsync())
        {
            context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(LeaveType.Annual, 20, 0, 5));
            context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(LeaveType.Sick, 30, 0, 0));
            context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(LeaveType.Personal, 3, 0, 0));
            await context.SaveChangesAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitmqContainer.StartAsync()
        );

        // Set environment variables for connection strings AFTER containers start
        Environment.SetEnvironmentVariable("ConnectionStrings__LeaveDbContext", _postgresContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitmqContainer.GetConnectionString());

        // Initialize database once for all tests
        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _rabbitmqContainer.DisposeAsync();
        _testRsa.Dispose();
    }
}




