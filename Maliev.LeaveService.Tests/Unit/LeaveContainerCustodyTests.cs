using Docker.DotNet.Models;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Moq;
using Maliev.LeaveService.Tests.TestUtilities;

namespace Maliev.LeaveService.Tests.Unit;

public sealed class LeaveContainerCustodyTests
{
    [Theory]
    [InlineData("postgres", 1073741824L, "/var/lib/postgresql")]
    [InlineData("rabbitmq", 536870912L, "/var/lib/rabbitmq")]
    [InlineData("redis", 268435456L, "/data")]
    public void DisposableFixtures_HaveFiniteResourcesAndLoopbackPorts(string kind, long memory, string dataPath)
    {
        var previous = Environment.GetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER");
        try
        {
            Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", "leave-unit-test");
            var custody = new LeaveContainerCustody();
            var parameters = new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>> { ["123/tcp"] = [new PortBinding { HostIP = "0.0.0.0" }] }
                }
            };
            custody.Configure(parameters, kind);
            Assert.Equal("leave-unit-test", parameters.Labels["maliev.test.owner"]);
            Assert.Equal("false", parameters.Labels["maliev.test.persistent-data"]);
            Assert.True(DateTimeOffset.Parse(parameters.Labels["maliev.test.expires-utc"]) > DateTimeOffset.UtcNow);
            Assert.Equal(memory, parameters.HostConfig.Memory);
            Assert.Equal(memory, parameters.HostConfig.MemorySwap);
            Assert.Equal(128, parameters.HostConfig.PidsLimit);
            Assert.Equal("127.0.0.1", parameters.HostConfig.PortBindings["123/tcp"][0].HostIP);
            Assert.Single(parameters.HostConfig.Tmpfs);
            Assert.Contains(dataPath, parameters.HostConfig.Tmpfs.Keys);
            Assert.Contains("timeout 1800", Assert.Single(parameters.Cmd));
        }
        finally { Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", previous); }
    }

    [Theory]
    [InlineData("missing-tmpfs")]
    [InlineData("memory")]
    [InlineData("cpu")]
    [InlineData("pids")]
    [InlineData("expiry")]
    [InlineData("foreign-volume")]
    public void ObservedFixtures_MustMatchAllOwnedResourceBounds(string mutation)
    {
        var previous = Environment.GetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER");
        try
        {
            Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", "leave-123-1");
            var custody = new LeaveContainerCustody();
            var parameters = new CreateContainerParameters { HostConfig = new HostConfig { PortBindings = new Dictionary<string, IList<PortBinding>>() } };
            custody.Configure(parameters, "postgres");
            var image = new DockerImage("postgres:18-alpine");
            var created = DateTime.UtcNow;
            var container = new Mock<IContainer>();
            container.SetupGet(value => value.Id).Returns(new string('a', 64));
            container.SetupGet(value => value.Image).Returns(image);
            container.SetupGet(value => value.CreatedTime).Returns(created);
            var actual = new ContainerInspectResponse
            {
                ID = container.Object.Id,
                Created = created,
                Config = new ContainerConfig { Image = image.FullName, Labels = parameters.Labels },
                HostConfig = parameters.HostConfig,
                Mounts = [new MountPoint { Type = "tmpfs", Destination = "/var/lib/postgresql" }],
                NetworkSettings = new NetworkSettings { Ports = new Dictionary<string, IList<PortBinding>>() }
            };
            custody.VerifyFixture(actual, container.Object);
            actual.Mounts.Clear();
            custody.VerifyFixture(actual, container.Object);
            actual.Mounts.Add(new MountPoint { Type = "tmpfs", Destination = "/var/lib/postgresql" });
            switch (mutation)
            {
                case "missing-tmpfs": actual.HostConfig.Tmpfs.Clear(); break;
                case "memory": actual.HostConfig.Memory--; break;
                case "cpu": actual.HostConfig.NanoCPUs--; break;
                case "pids": actual.HostConfig.PidsLimit--; break;
                case "expiry": actual.Config.Labels["maliev.test.expires-utc"] = "foreign"; break;
                case "foreign-volume": actual.Mounts.Add(new MountPoint { Type = "volume", Destination = "/foreign" }); break;
            }
            Assert.Throws<InvalidOperationException>(() => custody.VerifyFixture(actual, container.Object));
        }
        finally { Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", previous); }
    }

    [Fact]
    public void MissingOwner_IsRejectedBeforeAnyFixtureStarts()
    {
        var previous = Environment.GetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER");
        try
        {
            Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", null);
            Assert.Throws<InvalidOperationException>(() => new LeaveContainerCustody().Admit());
        }
        finally { Environment.SetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER", previous); }
    }
}
