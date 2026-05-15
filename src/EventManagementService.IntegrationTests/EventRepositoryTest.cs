using EventManagementService.DataAccess;
using EventManagementService.Filters;
using EventManagementService.Models;
using EventManagementService.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.IntegrationTests
{
    public class EventRepositoryTest : IAsyncLifetime
    {
        /// <summary>
        ///  Контейнер PostgreSQL для тестирования репозитория событий. 
        ///  Он использует образ "postgres:16-alpine"
        ///  будет автоматически запущен перед выполнением тестов и остановлен после их завершения.
        /// </summary>
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private AppDbContext CreateContext()
        {
            // Создание экземпляра AppDbContext с использованием строки подключения из контейнера PostgreSQL.
            var connectionString = _postgres.GetConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var context = new AppDbContext(options);

            // Создаёт таблицы по модели EF Core.
            context.Database.Migrate();

            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();

            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task AddEvent_ShouldAddEventToDatabase()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var @event = new Event
            {
                EventId = Guid.NewGuid(),
                Title = "Test Event",
                Description = "This is a test event.",
                //PostgreSQL тип timestamp with time zone всегда хранит время в UTC. Npgsql провайдер строго это проверяет
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 100,
                AvailableSeats = 100
            };

            // Act
            await repository.AddEventAsync(@event);
            await context.SaveChangesAsync();

            // Assert
            // Для проверки создаётся отдельный контекст
            // это исключает чтение из кеша и гарантирует, что данные реально записались в базу.
            await using var verifyContext = CreateContext();

            var retrievedEvent = await repository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().NotBeNull();
            retrievedEvent.EventId.Should().Be(@event.EventId);
            retrievedEvent.Title.Should().Be(@event.Title);
            retrievedEvent.Description.Should().Be(@event.Description);
            retrievedEvent.StartAt.Should().Be(@event.StartAt);
            retrievedEvent.EndAt.Should().Be(@event.EndAt);
            retrievedEvent.TotalSeats.Should().Be(@event.TotalSeats);
            retrievedEvent.AvailableSeats.Should().Be(@event.AvailableSeats);
        }

        [Fact]
        public async Task GetEventById_ShouldReturnEvent()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var @event = new Event
            {
                EventId = Guid.NewGuid(),
                Title = "Test Event",
                Description = "This is a test event.",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 100,
                AvailableSeats = 100
            };

            // Act
            await repository.AddEventAsync(@event);
            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var retrievedEvent = await repository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().NotBeNull();
            retrievedEvent.EventId.Should().Be(@event.EventId);
            retrievedEvent.Title.Should().Be(@event.Title);
            retrievedEvent.Description.Should().Be(@event.Description);
            retrievedEvent.StartAt.Should().Be(@event.StartAt);
            retrievedEvent.EndAt.Should().Be(@event.EndAt);
            retrievedEvent.TotalSeats.Should().Be(@event.TotalSeats);
            retrievedEvent.AvailableSeats.Should().Be(@event.AvailableSeats);
        }

        [Fact]
        public async Task RemoveEvent_ShouldRemoveEventFromDatabase()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var @event = new Event
            {
                EventId = Guid.NewGuid(),
                Title = "Test Event",
                Description = "This is a test event.",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 100,
                AvailableSeats = 100
            };

            // Act
            await repository.AddEventAsync(@event);
            await context.SaveChangesAsync();

            repository.RemoveEvent(@event);
            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var retrievedEvent = await repository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().BeNull();
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnAllEvents_WhenFilterIsEmpty()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter{};

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var pagedResult = await repository.GetEventsAsync(filter);

            pagedResult.TotalCount.Should().Be(4);
            pagedResult.Items.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_ByTitle() 
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                Title = "Conference",
                From = new DateTime(2026, 1, 1).ToUniversalTime(),
                To = new DateTime(2026, 12, 31).ToUniversalTime()
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var pagedResult = await repository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(1);
            pagedResult.Items.ElementAt(0).Title.Should().Contain("Conference");
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_ByDate()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                From = new DateTime(2026, 2, 11).ToUniversalTime(),
                To = new DateTime(2026, 4, 14).ToUniversalTime()
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var pagedResult = await repository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldApplyPagination()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            var pagedResult = await repository.GetEventsAsync(filter);

            pagedResult.TotalCount.Should().Be(4);
            pagedResult.Items.Should().HaveCount(2);
            pagedResult.Items.First().Title.Should().Be("DotNet Meetup");
            pagedResult.Items.Last().Title.Should().Be("Architecture Day");
        }

        private static List<Event> TestSeedData()
        {
            var events = new List<Event>
                {
                    new Event()
                    {
                        EventId = Guid.NewGuid(),
                        Title = "CSharp Conference",
                        StartAt = new DateTime(2026, 1, 10).ToUniversalTime(),
                        EndAt = new DateTime(2026, 1, 11).ToUniversalTime(),
                        TotalSeats = 100,
                        AvailableSeats = 100
                    },
                    new Event()
                    {
                        EventId = Guid.NewGuid(),
                        Title = "Java Summit",
                        StartAt = new DateTime(2026, 2, 11).ToUniversalTime(),
                        EndAt = new DateTime(2026, 2, 12).ToUniversalTime(),
                        TotalSeats = 100,
                        AvailableSeats= 100
                    },
                    new Event()
                    {
                        EventId = Guid.NewGuid(),
                        Title = "DotNet Meetup",
                        StartAt = new DateTime(2026, 3, 12).ToUniversalTime(),
                        EndAt = new DateTime(2026, 3, 13).ToUniversalTime(),
                        TotalSeats = 100,
                        AvailableSeats = 100
                    },
                    new Event()
                    {
                        EventId = Guid.NewGuid(),    
                        Title = "Architecture Day",
                        StartAt = new DateTime(2026, 4, 13).ToUniversalTime(),
                        EndAt = new DateTime(2026, 4, 14).ToUniversalTime(),
                        TotalSeats = 100,
                        AvailableSeats = 100
                    }
                };

            return events; 
        }
    }
}
