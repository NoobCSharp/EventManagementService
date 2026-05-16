using EventManagementService.DataAccess;
using EventManagementService.Enums;
using EventManagementService.Models;
using EventManagementService.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagementService.IntegrationTests
{
    public class BookingRepositoryTest : IAsyncLifetime
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
        public async Task CreateBooking_ShouldAddBooking_ToDatabase()
        {
            // Arrange (подготовка)
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

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

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await eventRepository.AddEventAsync(@event);
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(@event.EventId);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturnBooking_FromDatabase()
        {
            // Arrange (подготовка)
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

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

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await eventRepository.AddEventAsync(@event);
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(@event.EventId);
        }

        [Fact]
        public async Task GetPendingBookings_ShouldReturnBookings_FromDatabase_With_BookingStatus_Pending()
        {
            // Arrange (подготовка)
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

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

            var bookingOne = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Confirmed
            };

            var bookingTwo = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                Event = @event,
                Status = BookingStatus.Pending
            };

            var bookingThree = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            var bookingFourth = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Rejected
            };

            // Act (действие)
            await eventRepository.AddEventAsync(@event);

            await bookingRepository.CreateBookingAsync(bookingOne);
            await bookingRepository.CreateBookingAsync(bookingTwo);
            await bookingRepository.CreateBookingAsync(bookingThree);
            await bookingRepository.CreateBookingAsync(bookingFourth);


            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBookings = await verifyRepository.GetPendingBookingsAsync();

            retrievedBookings.Should().NotBeNull();

            retrievedBookings.Should()
                .HaveCount(2)
                .And.OnlyContain(b => b.Status == BookingStatus.Pending);

            retrievedBookings.Should()
                .Contain(b => b.BookingId == bookingTwo.BookingId);

            retrievedBookings.Should()
                .Contain(b => b.BookingId == bookingThree.BookingId);
        }
    }
}
