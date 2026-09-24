using Xunit;

namespace GiveAID.Tests.Integration.Fixtures;

[CollectionDefinition("ApiTests", DisableParallelization = true)]
public class ApiTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    // This class serves as the collection definition for API tests
    // All tests that share the same ApiWebApplicationFactory should use this collection
}
