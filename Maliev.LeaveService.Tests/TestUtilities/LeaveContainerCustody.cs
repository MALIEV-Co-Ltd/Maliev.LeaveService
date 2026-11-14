using System.Diagnostics;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Maliev.LeaveService.Tests.TestUtilities;

internal sealed class LeaveContainerCustody
{
    private readonly string _owner = Environment.GetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER") ?? "";
    private readonly string _fixture = Guid.NewGuid().ToString("N");
    private readonly string _expiry = DateTimeOffset.UtcNow.AddMinutes(35).ToString("O", CultureInfo.InvariantCulture);
    private readonly List<IContainer> _started = [];
    private readonly HashSet<string> _verifiedIds = new(StringComparer.Ordinal);
    private string? _receipt;
    private bool _settlementAttempted;
    private bool _settled;

    private void Record(object value)
    {
        if (_receipt is null) throw new InvalidOperationException("Fixture receipt not initialized.");
        File.AppendAllText(_receipt, JsonSerializer.Serialize(value) + "\n");
    }

    private void ObserveReaper(object? sender, ResourceReaperStateEventArgs args)
    {
        if (args.Instance.SessionId == ResourceReaper.DefaultSessionId)
            Record(new { type = "reaper-state", session = args.Instance.SessionId.ToString("D"), state = args.State.ToString() });
    }


    internal void Configure(CreateContainerParameters parameters, string kind)
    {
        parameters.Labels ??= new Dictionary<string, string>();
        parameters.Labels["maliev.test.owner"] = _owner;
        parameters.Labels["maliev.test.fixture"] = _fixture;
        parameters.Labels["maliev.test.expires-utc"] = _expiry;
        parameters.Labels["maliev.test.persistent-data"] = "false";
        parameters.HostConfig.Memory = kind switch { "postgres" => 1024L * 1024 * 1024, "rabbitmq" => 512L * 1024 * 1024, "redis" => 256L * 1024 * 1024, _ => throw new ArgumentOutOfRangeException(nameof(kind)) };
        parameters.HostConfig.MemorySwap = parameters.HostConfig.Memory;
        parameters.HostConfig.NanoCPUs = 750_000_000;
        parameters.HostConfig.PidsLimit = 128;
        var (path, limit, command) = kind switch
        {
            "postgres" => ("/var/lib/postgresql", "512m", "exec timeout 1800 docker-entrypoint.sh postgres"),
            "rabbitmq" => ("/var/lib/rabbitmq", "256m", "exec timeout 1800 docker-entrypoint.sh rabbitmq-server"),
            "redis" => ("/data", "64m", "exec timeout 1800 redis-server --save '' --appendonly no"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        parameters.HostConfig.Tmpfs = new Dictionary<string, string> { [path] = "rw,size=" + limit };
        parameters.Entrypoint = ["sh", "-c"];
        parameters.Cmd = [command];
        foreach (var bindings in parameters.HostConfig.PortBindings.Values)
            foreach (var binding in bindings) binding.HostIP = "127.0.0.1";
    }

    internal void Admit()
    {
        if (!OperatingSystem.IsLinux() || !Regex.IsMatch(_owner, @"\Aleave-[1-9][0-9]*-[1-9][0-9]*\z")
            || Environment.GetEnvironmentVariable("MALIEV_TEST_RESOURCE_OWNER") != _owner)
            throw new InvalidOperationException("Linux fixture owner admission is required.");
        var line = File.ReadLines("/proc/meminfo").Single(value => value.StartsWith("MemAvailable:", StringComparison.Ordinal));
        var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 3 || fields[2] != "kB" || !long.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var available) || available < 4194304)
            throw new InvalidOperationException("Fresh 4 GiB available physical memory is required.");
    }

    internal async Task StartAsync(params IContainer[] containers)
    {
        Admit();
        var ledger = Environment.GetEnvironmentVariable("MALIEV_RESOURCE_LEDGER");
        if (string.IsNullOrWhiteSpace(ledger) || !Directory.Exists(ledger))
            throw new InvalidOperationException("An existing task-owned resource ledger is required.");
        var session = ResourceReaper.DefaultSessionId.ToString("D");
        _receipt = Path.Combine(ledger, "fixture-" + _fixture + ".jsonl");
        using (new FileStream(_receipt, FileMode.CreateNew, FileAccess.Write, FileShare.Read)) { }
        using var process = Process.GetCurrentProcess();
        Record(new { type = "session", owner = _owner, fixture = _fixture, session, reaperName = "testcontainers-ryuk-" + session, processId = process.Id, processStartedUtc = process.StartTime.ToUniversalTime(), executable = Environment.ProcessPath, expiryUtc = _expiry, persistentData = false });
        ResourceReaper.StateChanged += ObserveReaper;
        try
        {
            foreach (var container in containers)
            {
                Admit();
                // Register before startup so a failed start is included in settlement.
                _started.Add(container);
                Record(new { type = "fixture-start-attempt", image = container.Image.FullName });
                container.Created += (_, _) => Record(new { type = "fixture-created", id = container.Id, createdUtc = container.CreatedTime, image = container.Image.FullName });
                using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
                await container.StartAsync(timeout.Token);
                using var docker = TestcontainersSettings.OS.DockerEndpointAuthConfig.GetDockerClientConfiguration(Guid.NewGuid()).CreateClient();
                using var observe = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var actual = await docker.Containers.InspectContainerAsync(container.Id, observe.Token);
                VerifyFixture(actual, container);
                _verifiedIds.Add(container.Id);
                Record(new { type = "fixture-observed", id = actual.ID, name = actual.Name, image = actual.Config.Image, createdUtc = actual.Created, mounts = actual.Mounts.Select(mount => new { mount.Type, mount.Destination }).ToArray(), ports = actual.NetworkSettings.Ports, memory = actual.HostConfig.Memory, cpu = actual.HostConfig.NanoCPUs, pids = actual.HostConfig.PidsLimit });
                var reaper = await docker.Containers.InspectContainerAsync("testcontainers-ryuk-" + session, observe.Token);
                if (reaper.Name != "/testcontainers-ryuk-" + session || reaper.Config.Image != "testcontainers/ryuk:0.14.0")
                    throw new InvalidOperationException("Exact session reaper identity unavailable.");
                Record(new { type = "reaper-observed", id = reaper.ID, name = reaper.Name, image = reaper.Config.Image, createdUtc = reaper.Created, mounts = reaper.Mounts.Select(mount => mount.Type).ToArray(), ports = reaper.NetworkSettings.Ports });
            }
        }
        catch (Exception primary)
        {
            try { await DisposeAsync(); }
            catch (Exception cleanup) { throw new AggregateException(primary, cleanup); }
            ExceptionDispatchInfo.Capture(primary).Throw();
            throw;
        }
        finally { ResourceReaper.StateChanged -= ObserveReaper; }
    }

    internal async Task DisposeAsync()
    {
        if (_settlementAttempted)
        {
            if (!_settled) throw new InvalidOperationException("Prior fixture cleanup remains unresolved; do not retry uncertain resources.");
            return;
        }
        _settlementAttempted = true;
        var failures = new List<Exception>();
        foreach (var container in _started.AsEnumerable().Reverse())
        {
            try { await ReleaseAsync(container); }
            catch (Exception error) { failures.Add(error); }
        }
        if (failures.Count > 0) throw new AggregateException("Fixture cleanup requires review; uncertain resources were preserved.", failures);
        _started.Clear();
        _settled = true;
    }

    internal void VerifyFixture(ContainerInspectResponse actual, IContainer container)
    {
        var (memory, path, size) = container.Image.FullName switch
        {
            var image when image.EndsWith("postgres:18-alpine", StringComparison.Ordinal) => (1024L * 1024 * 1024, "/var/lib/postgresql", "rw,size=512m"),
            var image when image.EndsWith("rabbitmq:4.2-alpine", StringComparison.Ordinal) => (512L * 1024 * 1024, "/var/lib/rabbitmq", "rw,size=256m"),
            var image when image.EndsWith("redis:8.4-alpine", StringComparison.Ordinal) => (256L * 1024 * 1024, "/data", "rw,size=64m"),
            _ => throw new InvalidOperationException("Unrecognized fixture image; preserve uncertain resource.")
        };
        if (actual.ID != container.Id || actual.Created != container.CreatedTime || actual.Config.Image != container.Image.FullName
            || !actual.Config.Labels.TryGetValue("maliev.test.owner", out var owner) || owner != _owner
            || !actual.Config.Labels.TryGetValue("maliev.test.fixture", out var fixture) || fixture != _fixture
            || !actual.Config.Labels.TryGetValue("maliev.test.persistent-data", out var persistent) || persistent != "false"
            || !actual.Config.Labels.TryGetValue("maliev.test.expires-utc", out var expiry) || expiry != _expiry
            || actual.HostConfig.Memory != memory || actual.HostConfig.MemorySwap != memory
            || actual.HostConfig.NanoCPUs != 750_000_000 || actual.HostConfig.PidsLimit != 128
            || actual.HostConfig.Tmpfs is null || actual.HostConfig.Tmpfs.Count != 1
            || !actual.HostConfig.Tmpfs.TryGetValue(path, out var options) || options != size
            || actual.Mounts.Count != 1 || actual.Mounts.Any(mount => mount.Type != "tmpfs" || mount.Destination != path)
            || actual.NetworkSettings.Ports.Values.Where(bindings => bindings is not null)
                .SelectMany(bindings => bindings).Any(binding => binding.HostIP != "127.0.0.1"))
            throw new InvalidOperationException("Exact disposable container custody could not be verified.");
    }

    private async Task ReleaseAsync(IContainer container)
    {
        if (container.State == TestcontainersStates.Undefined)
            throw new InvalidOperationException("Container creation was not settled; preserve uncertain custody.");
        var id = container.Id;
        using var docker = TestcontainersSettings.OS.DockerEndpointAuthConfig.GetDockerClientConfiguration(Guid.NewGuid()).CreateClient();
        using var observation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        ContainerInspectResponse actual;
        try { actual = await docker.Containers.InspectContainerAsync(id, observation.Token); }
        catch (DockerContainerNotFoundException)
        {
            if (!_verifiedIds.Contains(id))
                throw new InvalidOperationException("Container disappeared before mounts were observed; preserve uncertain custody.");
            await container.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(15));
            Record(new { type = "fixture-removed", id });
            return;
        }
        VerifyFixture(actual, container);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await container.StopAsync(stop.Token);
        await container.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(15));
        using var verify = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try { await docker.Containers.InspectContainerAsync(id, verify.Token); }
        catch (DockerContainerNotFoundException) { Record(new { type = "fixture-removed", id }); return; }
        throw new InvalidOperationException("Owned fixture container remains after disposal.");
    }
}
