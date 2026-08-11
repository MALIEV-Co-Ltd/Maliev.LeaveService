namespace Maliev.LeaveService.Tests.Unit;

public sealed class WorkflowContractTests
{
    private const string CheckoutSha = "3d3c42e5aac5ba805825da76410c181273ba90b1";
    private const string SetupDotnetSha = "a98b56852c35b8e3190ac28c8c2271da59106c68";
    private const string AspireSha = "d79f954b2e7224b48bec75af4826bb38dd5f2a20";
    private const string MessagingSha = "2347023e8aa17dfd7071317c2ee5c4436e1d945a";
    private const string GitleaksChecksum = "551f6fc83ea457d62a0d98237cbad105af8d557003051f41f3e7ca7b3f2470eb";

    private static readonly string Root = FindRoot();
    private static readonly string Workflows = Path.Combine(Root, ".github", "workflows");

    [Fact]
    public void PullRequests_AlwaysUseReadOnlyReusableValidation()
    {
        var source = ReadWorkflow("pr-validation.yml");
        Assert.Contains("pull_request:", source, StringComparison.Ordinal);
        Assert.Contains("contents: read", source, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", source, StringComparison.Ordinal);
        Assert.DoesNotContain("paths:", source, StringComparison.Ordinal);
        AssertSafe(source);
    }

    [Theory]
    [InlineData("ci-main.yml", "main")]
    [InlineData("ci-develop.yml", "develop")]
    [InlineData("ci-staging.yml", "release/v*")]
    public void BranchAndTagWorkflows_AreValidationOnly(string file, string trigger)
    {
        var source = ReadWorkflow(file);
        Assert.Contains(trigger, source, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", source, StringComparison.Ordinal);
        AssertSafe(source);
    }

    [Fact]
    public void ReusableValidation_UsesImmutablePublicSharedSources()
    {
        var source = ReadWorkflow("_validate.yml");
        Assert.Contains("workflow_call:", source, StringComparison.Ordinal);
        Assert.Contains("name: validate", source, StringComparison.Ordinal);
        Assert.Contains($"actions/checkout@{CheckoutSha}", source, StringComparison.Ordinal);
        Assert.Contains($"actions/setup-dotnet@{SetupDotnetSha}", source, StringComparison.Ordinal);
        Assert.Contains($"ref: {AspireSha}", source, StringComparison.Ordinal);
        Assert.Contains($"ref: {MessagingSha}", source, StringComparison.Ordinal);
        Assert.Contains("GITHUB_ACTIONS=false dotnet restore", source, StringComparison.Ordinal);
        Assert.Contains("--configfile nuget.validation.config", source, StringComparison.Ordinal);

        var nuget = File.ReadAllText(Path.Combine(Root, "nuget.validation.config"));
        Assert.Contains("<clear />", nuget, StringComparison.Ordinal);
        Assert.Contains("https://api.nuget.org/v3/index.json", nuget, StringComparison.Ordinal);
        Assert.DoesNotContain("nuget.pkg.github.com", nuget, StringComparison.OrdinalIgnoreCase);
        AssertSafe(source);
    }

    [Fact]
    public void ReusableValidation_UsesPinnedOssGitleaksWithoutLicenseSecret()
    {
        var source = ReadWorkflow("_validate.yml");
        Assert.Contains("gitleaks_8.30.1_linux_x64.tar.gz", source, StringComparison.Ordinal);
        Assert.Contains(GitleaksChecksum, source, StringComparison.Ordinal);
        Assert.Contains("./gitleaks dir service --no-banner --redact", source, StringComparison.Ordinal);
        Assert.DoesNotContain("gitleaks/gitleaks-action", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GITLEAKS_LICENSE", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DependabotConfiguration_HasParserSafeV2Root()
    {
        var bytes = File.ReadAllBytes(Path.Combine(Root, ".github", "dependabot.yml"));
        Assert.False(bytes.AsSpan().StartsWith(System.Text.Encoding.UTF8.Preamble));
        var source = System.Text.Encoding.UTF8.GetString(bytes).ReplaceLineEndings("\n");
        Assert.StartsWith("version: 2\nupdates:\n", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryWorkflow_ForbidsCredentialsAndDeploymentMutation()
    {
        foreach (var file in Directory.GetFiles(Workflows, "*.yml"))
        {
            AssertSafe(File.ReadAllText(file));
        }
    }

    private static void AssertSafe(string source)
    {
        foreach (var forbidden in new[]
        {
            "secrets.", "GITOPS_PAT", "GCP_SA_KEY", "NUGET_PASSWORD", "GITLEAKS_LICENSE",
            "id-token: write", "credentials_json", "google-github-actions/auth", "gcloud auth",
            "docker push", "maliev-gitops", "kustomize edit", "git push origin", "gh pr create",
            "pull_request_target",
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ReadWorkflow(string file)
    {
        var path = Path.Combine(Workflows, file);
        Assert.True(File.Exists(path), $"Required workflow is missing: {file}");
        return File.ReadAllText(path);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.LeaveService.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate LeaveService repository root.");
    }
}
