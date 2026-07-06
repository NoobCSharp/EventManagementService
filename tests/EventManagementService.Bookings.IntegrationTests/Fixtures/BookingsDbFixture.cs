using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.Identity.IntegrationTests.Fixtures
{
    public sealed class BookingsDbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }
        private string _connectionString = string.Empty;

        public BookingsDbFixture()
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
                "TRUNCATE TABLE \"Bookings\" RESTART IDENTITY CASCADE");
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }
}
