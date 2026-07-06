using EventManagement.Bookings.Domain.Entities;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Bookings.Infrastructure.Repositories;
using EventManagementService.Identity.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventManagementService.IntegrationTests
{
    public class BookingRepositoryTest : IClassFixture<BookingsDbFixture>
    {
        private readonly BookingsDbFixture _fixture;

        public BookingRepositoryTest(BookingsDbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateBooking_ShouldAddBooking_ToDatabase()
        {
            // Arrange (подготовка)
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var bookingRepository = new BookingRepository(context);

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(eventId);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturnBooking_FromDatabase()
        {
            // Arrange (подготовка)
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var bookingRepository = new BookingRepository(context);

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Status = BookingStatus.Pending
            };

            // Act (действие)
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert (проверка)
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new BookingRepository(verifyContext);
            var retrievedBooking = await verifyRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(eventId);
        }

        [Fact]
        public async Task GetPendingBookings_ShouldReturnBookings_FromDatabase_With_BookingStatus_Pending()
        {
            // Arrange (подготовка)
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var bookingRepository = new BookingRepository(context);

            var bookingOne = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Status = BookingStatus.Confirmed
            };

            var bookingTwo = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                Status = BookingStatus.Pending
            };

            var bookingThree = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Status = BookingStatus.Pending
            };

            var bookingFourth = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Status = BookingStatus.Rejected
            };

            // Act (действие)
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
