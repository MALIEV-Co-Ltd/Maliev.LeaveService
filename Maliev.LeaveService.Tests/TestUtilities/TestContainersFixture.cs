using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Maliev.LeaveService.Tests.TestUtilities;

public class TestContainersFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("leave_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public RabbitMqContainer RabbitMqContainer { get; } = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public RedisContainer RedisContainer { get; } = new RedisBuilder()
        .WithImage("redis:7")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.StartAsync(),
            RabbitMqContainer.StartAsync(),
            RedisContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.DisposeAsync().AsTask(),
            RabbitMqContainer.DisposeAsync().AsTask(),
            RedisContainer.DisposeAsync().AsTask()
        );
    }
}
