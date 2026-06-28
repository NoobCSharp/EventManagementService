using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;

namespace EventManagementService.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class BookingRepositoryTest
    {
        private readonly PostgreSqlContainerFixture _fixture;

        public BookingRepositoryTest(PostgreSqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateBooking_ShouldAddBooking_ToDatabase()
        {
            // Arrange (подготовка)
            await _fixture.ResetDatabaseAsync();

            await using var context = _fixture.CreateContext();

            var userRepository = new UserRepository(context);
            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = "Test User",
                PasswordHash = "abcd",
                Role = Role.User
            };

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
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await userRepository.AddUserAsync(user);
            await eventRepository.AddEventAsync(@event);
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(@event.EventId);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturnBooking_FromDatabase()
        {
            // Arrange (подготовка)
            await _fixture.ResetDatabaseAsync();

            await using var context = _fixture.CreateContext();

            var userRepository = new UserRepository(context);
            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = "Test User",
                PasswordHash = "abcd",
                Role = Role.User
            };

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
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await userRepository.AddUserAsync(user);
            await eventRepository.AddEventAsync(@event);
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(@event.EventId);
        }

        [Fact]
        public async Task GetPendingBookings_ShouldReturnBookings_FromDatabase_With_BookingStatus_Pending()
        {
            // Arrange (подготовка)
            await _fixture.ResetDatabaseAsync();

            await using var context = _fixture.CreateContext();

            var userRepository = new UserRepository(context);
            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = "Test User",
                PasswordHash = "abcd",
                Role = Role.User
            };

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
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Confirmed
            };

            var bookingTwo = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                Event = @event,
                Status = BookingStatus.Pending
            };

            var bookingThree = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            var bookingFourth = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Rejected
            };

            // Act (действие)
            await userRepository.AddUserAsync(user);
            await eventRepository.AddEventAsync(@event);

            await bookingRepository.CreateBookingAsync(bookingOne);
            await bookingRepository.CreateBookingAsync(bookingTwo);
            await bookingRepository.CreateBookingAsync(bookingThree);
            await bookingRepository.CreateBookingAsync(bookingFourth);


            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = _fixture.CreateContext();

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
