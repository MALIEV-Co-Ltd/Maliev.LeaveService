using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Maliev.LeaveService.Tests.TestUtilities;

public class TestContainersFixture : IAsyncLifetime
{
    private readonly LeaveContainerCustody _custody = new();
    public PostgreSqlContainer PostgreSqlContainer { get; }
    public RabbitMqContainer RabbitMqContainer { get; }
    public RedisContainer RedisContainer { get; }

    public TestContainersFixture()
    {
#pragma warning disable CS0618
        PostgreSqlContainer = new PostgreSqlBuilder().WithImage("postgres:18-alpine")
            .WithDatabase("leave_db").WithUsername("postgres").WithPassword("postgres")
            .WithCreateParameterModifier(parameters => _custody.Configure(parameters, "postgres")).Build();
        RabbitMqContainer = new RabbitMqBuilder().WithImage("rabbitmq:4.2-alpine")
            .WithUsername("guest").WithPassword("guest")
            .WithCreateParameterModifier(parameters => _custody.Configure(parameters, "rabbitmq")).Build();
        RedisContainer = new RedisBuilder().WithImage("redis:8.4-alpine")
            .WithCreateParameterModifier(parameters => _custody.Configure(parameters, "redis")).Build();
#pragma warning restore CS0618
    }

    public Task InitializeAsync() => _custody.StartAsync(PostgreSqlContainer, RabbitMqContainer, RedisContainer);
    public Task DisposeAsync() => _custody.DisposeAsync();
}
