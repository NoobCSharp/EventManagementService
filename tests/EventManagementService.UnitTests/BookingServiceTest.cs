using EventManagement.Bookings.Application.Dtos;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Options;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Domain.Entities;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventManagementService.UnitTests
{
    public class BookingServiceTest
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();
        private readonly Mock<IKafkaProducer> _kafkaProducerMock = new();

        private BookingService CreateBookingService()
        {
            var bookingSettings = Options.Create(new BookingOptions
            {
                ActiveBookingsLimit = 10
            });

            return new BookingService(
                _bookingRepositoryMock.Object,
                _kafkaProducerMock.Object,
                bookingSettings
            );
        }

        #region Successful scenarios for BookingService

        [Fact]
        public async Task GetBookingById_ShouldReturn_ExistingBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                SeatCount = 1,
                CreatedAt = DateTime.UtcNow
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var service = CreateBookingService();

            // Act
            var result = await service.GetBookingByIdAsync(bookingId);

            // Assert
            result.Should().NotBeNull();
            result.BookingId.Should().Be(bookingId);

            _bookingRepositoryMock.Verify(r =>
                r.GetBookingByIdAsync(bookingId),
                Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_Should_CreateBooking_AndPublishBookingCreatedMessage()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new BookingCreateDtoRequest() 
            { 
                SeatCount = 1 
            };

            var service = CreateBookingService();

            // Act
            var response = await service.CreateBookingAsync(eventId, userId, request);

            // Assert
            response.Should().NotBeNull();
            response.EventId.Should().Be(eventId);

            _bookingRepositoryMock.Verify(r => r.CreateBookingAsync(
                It.Is<Booking>(b =>
                    b.EventId == eventId &&
                    b.UserId == userId &&
                    b.Status == BookingStatus.Pending)),
                Times.Once);

            _bookingRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            _kafkaProducerMock.Verify(p => p.ProduceAsync(
                KafkaTopics.BookingCreated,
                It.Is<BookingCreatedMessage>(m =>
                    m.BookingId == response.BookingId &&
                    m.EventId == eventId &&
                    m.UserId == userId),
                eventId.ToString(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_Should_PublishBookingCancelledMessage()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SeatCount = 1,
                Status = BookingStatus.Pending
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var service = CreateBookingService();

            // Act
            await service.CancelBookingAsync(bookingId, userId, Role.User);

            // Assert
            _kafkaProducerMock.Verify(p => p.ProduceAsync(
                KafkaTopics.BookingCancelled,
                It.Is<BookingCancelledMessage>(m =>
                    m.BookingId == bookingId &&
                    m.EventId == eventId),
                eventId.ToString(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_Admin_ShouldPublishBookingCancelledMessage()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SeatCount = 1,
                Status = BookingStatus.Pending
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var service = CreateBookingService();

            // Act
            await service.CancelBookingAsync(bookingId, adminId, Role.Admin);

            // Assert
            _kafkaProducerMock.Verify(p => p.ProduceAsync(
                KafkaTopics.BookingCancelled,
                It.Is<BookingCancelledMessage>(m =>
                    m.BookingId == bookingId &&
                    m.EventId == eventId),
                eventId.ToString(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Unsuccessful scenarios for BookingService

        [Fact]
        public async Task GetBookingById_WithNonExistingId_ShouldThrow_BookingNotFoundException()
        {
            // Arrange (подготовка)
            var bookingId = Guid.NewGuid();

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking?)null);

            var service = CreateBookingService();

            // Assert (проверка)
            await service
                .Invoking(s => s.GetBookingByIdAsync(bookingId))
                .Should()
                .ThrowAsync<BookingNotFoundException>();
        }

        [Fact]
        public async Task CreateBookingAsync_WhenBookingLimitIsReached_ShouldThrow_ActiveBookingLimitExceededException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new BookingCreateDtoRequest()
            {
                SeatCount = 1
            };

            _bookingRepositoryMock
                .Setup(r => r.GetActiveBookingsCountAsync(userId))
                .ReturnsAsync(10);

            var service = CreateBookingService();

            // Act & Assert
            await service.Invoking(s =>
                s.CreateBookingAsync(eventId, userId, request))
                .Should()
                .ThrowAsync<ActiveBookingLimitExceededException>();
        }

        [Fact]
        public async Task CancelBooking_With_BookingStatusCancelled_ShouldThrow_BookingValidationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SeatCount = 1,
                Status = BookingStatus.Cancelled
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var service = CreateBookingService();

            // Act & Assert
            await service.Invoking(s =>
                s.CancelBookingAsync(bookingId, userId, Role.User))
                .Should()
                .ThrowAsync<BookingValidationException>();
        }

        [Fact]
        public async Task CreateBooking_With_SeatCountLessOrEqualZero_ShouldThrow_BookingValidationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var request = new BookingCreateDtoRequest()
            {
                SeatCount = 0
            };

            var service = CreateBookingService();

            // Act & Assert
            await service.Invoking(s =>
                s.CreateBookingAsync(eventId, bookingId, request))
                .Should()
                .ThrowAsync<BookingValidationException>();
        }

        [Fact]
        public async Task CancelBooking_ByUserWhoDoesNotOwnBooking_ShouldThrow_UnauthorizedAccessException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SeatCount= 1,
                Status = BookingStatus.Confirmed
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var service = CreateBookingService();

            // Act & Assert
            await service.Invoking(s =>
                s.CancelBookingAsync(bookingId, otherUserId, Role.User))
                .Should()
                .ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CancelBookingAsync_WithNonExistingId_ShouldThrow_BookingNotFoundException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking?)null);

            var service = CreateBookingService();

            // Act & Assert
            await service.Invoking(s =>
                s.CancelBookingAsync(bookingId, userId, Role.User))
                .Should()
                .ThrowAsync<BookingNotFoundException>();

            _kafkaProducerMock.Verify(p => p.ProduceAsync(
                KafkaTopics.BookingCancelled,
                It.IsAny<BookingCancelledMessage>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion
    }
}
