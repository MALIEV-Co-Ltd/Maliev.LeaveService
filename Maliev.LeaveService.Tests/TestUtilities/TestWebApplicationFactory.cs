using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder().WithImage("postgres:18-alpine").Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder().WithImage("redis:7-alpine").Build();
    private readonly RabbitMqContainer _rabbitmqContainer = new RabbitMqBuilder().WithImage("rabbitmq:4-management-alpine").Build();
    private readonly RSA _testRsa = RSA.Create(2048);

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

        // Set environment variables for connection strings (read early in configuration pipeline)
        Environment.SetEnvironmentVariable("ConnectionStrings__LeaveDbContext", _postgresContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitmqContainer.GetConnectionString());

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

            services.AddMassTransitTestHarness();
        });
    }

    [SuppressMessage("Security", "EF1002:Gaps in SQL queries", Justification = "Table names are known constants and are safe.")]
    public async Task ResetDatabaseAsync()
    {
        // Ensure the host is built by accessing Server property
        _ = Server;

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        // Truncate all tables
        var tableNames = new[] { "leave_approvals", "leave_requests", "leave_balances", "leave_policies" };
        foreach (var table in tableNames)
        {
            await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE");
        }

        // Seed default leave policies
        await SeedDefaultPoliciesAsync(context);
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
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _rabbitmqContainer.DisposeAsync();
        _testRsa.Dispose();
    }
}