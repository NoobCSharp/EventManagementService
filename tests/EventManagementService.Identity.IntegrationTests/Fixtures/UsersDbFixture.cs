using EventManagement.Identity.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.Identity.IntegrationTests.Fixtures
{
    public sealed class UsersDbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }
        private string _connectionString = string.Empty;

        public UsersDbFixture()
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

        public IdentityDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return new IdentityDbContext(options);
        }

        public async Task ResetAsync()
        {
            await using var context = CreateContext();

            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE");
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }
}
