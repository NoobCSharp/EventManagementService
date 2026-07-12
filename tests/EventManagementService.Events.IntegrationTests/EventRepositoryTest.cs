using EventManagement.Events.Domain.Entities;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Infrastructure.Repositories;
using EventManagementService.Events.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventManagementService.Events.IntegrationTests
{
    public class EventRepositoryTest : IClassFixture<EventsDbFixture>
    {
        private readonly EventsDbFixture _fixture;

        public EventRepositoryTest(EventsDbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddEvent_ShouldAddEventToDatabase()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

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
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var retrievedEvent = await verifyRepository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().NotBeNull();
            retrievedEvent.EventId.Should().Be(@event.EventId);
            retrievedEvent.Title.Should().Be(@event.Title);
            retrievedEvent.Description.Should().Be(@event.Description);
            retrievedEvent.StartAt.Should().BeCloseTo(@event.StartAt, TimeSpan.FromMicroseconds(1));
            retrievedEvent.EndAt.Should().BeCloseTo(@event.EndAt, TimeSpan.FromMicroseconds(1));
            retrievedEvent.TotalSeats.Should().Be(@event.TotalSeats);
            retrievedEvent.AvailableSeats.Should().Be(@event.AvailableSeats);
        }

        [Fact]
        public async Task GetEventById_ShouldReturnEvent()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

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
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var retrievedEvent = await verifyRepository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().NotBeNull();
            retrievedEvent.EventId.Should().Be(@event.EventId);
            retrievedEvent.Title.Should().Be(@event.Title);
            retrievedEvent.Description.Should().Be(@event.Description);
            retrievedEvent.StartAt.Should().BeCloseTo(@event.StartAt, TimeSpan.FromMicroseconds(1));
            retrievedEvent.EndAt.Should().BeCloseTo(@event.EndAt, TimeSpan.FromMicroseconds(1));
            retrievedEvent.TotalSeats.Should().Be(@event.TotalSeats);
            retrievedEvent.AvailableSeats.Should().Be(@event.AvailableSeats);
        }

        [Fact]
        public async Task RemoveEvent_ShouldRemoveEventFromDatabase()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

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
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var retrievedEvent = await verifyRepository.GetEventByIdAsync(@event.EventId);

            retrievedEvent.Should().BeNull();
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnAllEvents_WhenFilterIsEmpty()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter{};

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

            pagedResult.TotalCount.Should().Be(4);
            pagedResult.Items.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_OnlyByTitle() 
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                Title = "Conference",
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(1);
            pagedResult.Items.ElementAt(0).Title.Should().Contain("Conference");
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_OnlyByFrom()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                From = new DateTime(2026, 3, 12).ToUniversalTime(),
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(2);
            pagedResult.Items.Should().OnlyContain(e => e.StartAt >= filter.From);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_OnlyByTo()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new EventRepository(context);

            var filter = new EventFilter
            {
                To = new DateTime(2026, 3, 12).ToUniversalTime(),
            };

            // Act
            context.Events.AddRange(TestSeedData());

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(2);
            pagedResult.Items.Should().OnlyContain(e => e.EndAt <= filter.To);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEventsWithFilter_OnlyByDate()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

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
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

            pagedResult.Items.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEvents_WithPagination()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

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
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new EventRepository(verifyContext);
            var pagedResult = await verifyRepository.GetEventsAsync(filter);

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
