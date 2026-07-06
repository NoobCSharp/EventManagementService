using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.Events.IntegrationTests.Fixtures
{
    public sealed class EventsDbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }
        private string _connectionString = string.Empty;

        public EventsDbFixture()
        {
            Container = new PostgreSqlBuilder("postgres:16").Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            _connectionString = Container.GetConnectionString();

            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return new AppDbContext(options);
        }

        public async Task ResetAsync()
        {
            await using var context = CreateContext();

            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Events\" RESTART IDENTITY CASCADE");
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }
}
