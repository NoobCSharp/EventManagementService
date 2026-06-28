namespace EventManagementService.IntegrationTests
{
    [CollectionDefinition("PostgresCollection")]
    public class PostgresCollectionFixture : ICollectionFixture<PostgreSqlContainerFixture>
    {
    }
}