using EventManagementService.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.IntegrationTests
{
    public sealed class PostgreSqlContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }

        public PostgreSqlContainerFixture()
        {
            Container = new PostgreSqlBuilder("postgres:16-alpine").Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            await using var context = CreateContext();

            // Создаёт таблицы по модели EF Core путем миграции.
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }

        public AppDbContext CreateContext()
        {
            // Создание экземпляра AppDbContext с использованием строки подключения из контейнера PostgreSQL.
            var connectionString = Container.GetConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        public async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();

            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE");
        }
    }
}
