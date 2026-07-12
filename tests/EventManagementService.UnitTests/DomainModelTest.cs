using EventManagement.Bookings.Domain.Entities;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Events.Domain.Entities;
using FluentAssertions;

namespace EventManagementService.UnitTests
{
    public class DomainModelTest
    {
        /// <summary>
        /// Проверяет, что Confirm устанавливает статус Confirmed
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public async Task ConfirmBooking_Should_Set_StatusConfirmed_And_ProcessedAt()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var processedAt = DateTime.UtcNow.AddMinutes(5);

            var fakeBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = processedAt,
                SeatCount = 1,
                Status = BookingStatus.Pending,
            };

            // Act
            fakeBooking.Confirm(processedAt);

            // Assert
            fakeBooking.Status.Should().Be(BookingStatus.Confirmed);
            fakeBooking.ProcessedAt.Should().Be(processedAt);
        }

        /// <summary>
        /// Проверяет, что Reject устанавливает статус Rejected
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public async Task RejectBooking_Should_Set_StatusReject_And_ProcessedAt()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var processedAt = DateTime.UtcNow.AddMinutes(5);

            var fakeBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = processedAt,
                SeatCount= 1,
                Status = BookingStatus.Pending,
            };

            // Act
            fakeBooking.Reject(processedAt);

            // Assert
            fakeBooking.Status.Should().Be(BookingStatus.Rejected);
            fakeBooking.ProcessedAt.Should().Be(processedAt);
        }

        /// <summary>
        /// Проверяет, что  при отклонении брони и освобождении места
        /// количество доступных мест увеличивается.
        /// </summary>
        [Fact]
        public async Task ReleaseSeats_Should_Release_AvailableSeats()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 1
            };

            // Act
            fakeEvent.ReleaseSeats(5);

            // Assert
            fakeEvent.AvailableSeats.Should().Be(6);
        }

        /// <summary>
        /// Проверяет, что Cancel устанавливает статус Cancelled
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public async Task CancelBooking_Should_Set_StatusCancelled_And_ProcessedAt()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var processedAt = DateTime.UtcNow.AddMinutes(5);

            var fakeBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = processedAt,
                SeatCount = 1,
                Status = BookingStatus.Confirmed,
            };

            // Act
            fakeBooking.Cancel(processedAt);

            // Assert
            fakeBooking.Status.Should().Be(BookingStatus.Cancelled);
            fakeBooking.ProcessedAt.Should().Be(processedAt);
        }
    }
}
