using Xunit;
using Maliev.LeaveService.Tests.TestUtilities;

namespace Maliev.LeaveService.Tests.Integration;

/// <summary>
/// Defines a test collection for integration tests.
/// All tests in this collection will run sequentially and share the same test containers.
/// </summary>
[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    // This class is never instantiated. It exists solely to define the collection.
}
