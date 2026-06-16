using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventManagementService.UnitTests
{
    public class BookingServiceTest
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private BookingService CreateBookingService()
        {
            var bookingSettings = Options.Create(new BookingSettings
            {
                MaxActiveBookings = 10
            });

            return new BookingService(
                _eventRepositoryMock.Object,
                _bookingRepositoryMock.Object,
                _unitOfWorkMock.Object,
                bookingSettings
            );
        }

        #region Successful scenarios for BookingService

        /// <summary>
        /// Проверяет, что при создании брони она имеет корректные данные
        /// статуса и привязана к конкретному событию и пользователю по Id
        /// </summary>
        [Fact]
        public async Task AddBooking_Should_AddBooking()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            _eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            var response = await service.CreateBookingAsync(eventId, userId);

            // Assert (проверка)
            response.Should().NotBeNull();

            response.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            response.EventId.Should().Be(eventId);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.Is<Booking>(
                        b => b.EventId == eventId 
                        && b.UserId == userId 
                        && b.Status == BookingStatus.Pending)),
                Times.Once);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что можно создать несколько броней одним пользователем для одного события,
        /// и каждая имеет уникальный Id и статус Pending.
        /// </summary>
        [Fact]
        public async Task AddManyBooking_ShouldAdd_ManyBooking_ToOneEvent()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            _eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            var bookingFirst = await service.CreateBookingAsync(eventId, userId);
            var bookingLast = await service.CreateBookingAsync(eventId, userId);

            // Assert (проверка)
            bookingFirst.EventId.Should().Be(eventId);
            bookingLast.EventId.Should().Be(eventId);

            bookingFirst.Status.Should().Be(BookingStatus.Pending);
            bookingLast.Status.Should().Be(BookingStatus.Pending);

            bookingFirst.BookingId.Should().NotBe(bookingLast.BookingId);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.Is<Booking>(b =>
                        b.EventId == eventId 
                        && b.UserId == userId
                        && b.Status == BookingStatus.Pending)),
                Times.Exactly(2));

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Exactly(2));
        }

        /// <summary>
        /// Проверяет, что получение брони по Id возвращает корректный объект.
        /// </summary>
        [Fact]
        public async Task GetBookingById_ShouldReturn_Booking()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            var bookingId = Guid.NewGuid();

            var fakeBooking = new Booking
            {
                 BookingId = bookingId,
                 EventId = eventId,
                 UserId = userId, 
                 Event = fakeEvent,
                 CreatedAt = DateTime.UtcNow,
                 ProcessedAt = DateTime.UtcNow,
                 Status = BookingStatus.Pending,
            };

            _bookingRepositoryMock.Setup(
                r => r.GetBookingByIdAsync(bookingId)).
                ReturnsAsync(fakeBooking);

            var service = CreateBookingService();

            // Act (действие)
            var response = await service.GetBookingByIdAsync(bookingId);

            // Assert (проверка)
            response.Should().NotBeNull();

            response.BookingId.Should().Be(bookingId);
            response.EventId.Should().Be(eventId);
            response.UserId.Should().Be(userId);

            response.Status.Should().Be(BookingStatus.Pending);

            _bookingRepositoryMock.Verify(
                r => r.GetBookingByIdAsync(bookingId),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что при создании брони количество доступных мест уменьшается на 1.
        /// </summary>
        [Fact]
        public async Task CreatingBooking_ShouldReduces_AvailableSeats_By_1()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            _eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            var response = await service.CreateBookingAsync(eventId, userId);

            // Assert (проверка)
            fakeEvent.AvailableSeats.Should().Be(9);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.IsAny<Booking>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что при создании нескольких броней до лимита
        /// все операции успешны и каждая бронь имеет уникальный Id.
        /// </summary>
        [Fact]
        public async Task CreatingMultipleBookings_ToLimit_AllSuccessful_EachHasUniqueId()
        {
            // Arrange (подготовка)
            var limit = 5;

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid(); 

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            _eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            var tasks = Enumerable.Range(0, limit)
                .Select(_ => Task.Run(async () =>
                {
                    return await service.CreateBookingAsync(eventId, userId);
                }));

            var results = await Task.WhenAll(tasks);

            // Assert (проверка)
            results.Should()
                .HaveCount(limit);

            results.Should()
                .OnlyContain(b =>
                    b.EventId == eventId &&
                    b.Status == BookingStatus.Pending);

            results
                .Select(b => b.BookingId)
                .Should()
                .OnlyHaveUniqueItems();

            fakeEvent.AvailableSeats.Should().Be(5);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(limit));

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Exactly(limit));
        }

        /// <summary>
        /// Проверяет, что после исчерпания доступных мест
        /// следующая попытка бронирования выбрасывает NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_AfterRunning_OutOfPlaces_NextAttempt_ShouldThrow_NoAvailableSeatsException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 2,
                AvailableSeats = 1
            };

            _eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException после исчерпания мест
            var tasks = Enumerable.Range(0, 2)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await service.CreateBookingAsync(eventId, userId);
                        return (Success: true, Exception: (Exception?)null);
                    }
                    catch (Exception ex)
                    {
                        return (Success: false, Exception: ex);
                    }
                }));

            var results = await Task.WhenAll(tasks);

            results.Should().ContainSingle(r => r.Success);
            results.Should().ContainSingle(r => r.Exception is NoAvailableSeatsException);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что при отклонении брони
        /// количество свободных мест увеличивается.
        /// </summary>
        [Fact]
        public async Task RejectBooking_Should_Release_AvailableSeats()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 0
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            booking.Reject(DateTime.UtcNow);

            fakeEvent.ReleaseSeats();

            // Assert (проверка)
            booking.Status.Should().Be(BookingStatus.Rejected);

            fakeEvent.AvailableSeats.Should().Be(1);
        }

        /// <summary>
        /// Проверяет, что при отмене брони статус изменяется на Canceled и
        /// количество свободных мест у события восстанавливается.
        /// </summary>
        [Fact]
        public async Task CancelBooking_Should_ReleaseEventSeats()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 1,
                AvailableSeats = 0
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            await service.CancelBookingAsync(bookingId, userId, Role.User);

            // Assert (проверка)
            booking.Status.Should().Be(BookingStatus.Cancelled);

            fakeEvent.AvailableSeats.Should().Be(1);
        }

        /// <summary>
        /// Проверяет, что после отклонения брони и освобождения места
        /// можно создать новую бронь для того же события.
        /// </summary>
        [Fact]
        public async Task After_ReleaseSeats_Should_Allow_New_Booking()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var bookingId = Guid.NewGuid();

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            _bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = CreateBookingService();

            // Act (действие)
            var firstResult = await service.CreateBookingAsync(eventId, userId);

            fakeEvent.ReleaseSeats();

            var secondResult = await service.CreateBookingAsync(eventId, userId);

            // Assert (проверка)
            firstResult.Should().NotBeNull();
            secondResult.Should().NotBeNull();

            firstResult.BookingId.Should().NotBe(secondResult.BookingId);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(2));

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Exactly(2));
        }

        /// <summary>
        /// Проверяет защиту от переполнения (overbooking) при многопоточных запросах:
        /// количество успешных броней не превышает доступные места,
        /// остальные попытки завершаются с NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task Overbooking_Protection_Test()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 5,
                AvailableSeats = 5
            };

            var bookingId = Guid.NewGuid();

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            _bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = CreateBookingService();

            var requestsCount = 20;

            // Act (действие)
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await service.CreateBookingAsync(eventId, userId);
                        return (Success: true, Exception: (Exception?)null);
                    }
                    catch (Exception ex)
                    {
                        return (Success: false, Exception: ex);
                    }
                }));

            var results = await Task.WhenAll(tasks);

            // Assert (проверка)
            results.Count(r => r.Success).Should().Be(5);
            results.Count(r => r.Exception is NoAvailableSeatsException).Should().Be(15);

            fakeEvent.AvailableSeats.Should().Be(0);

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(5));

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Exactly(5));
        }

        /// <summary>
        /// Проверяет защиту от переполнения (overbooking) при многопоточных запросах:
        /// количество успешных броней не превышает доступные места,
        /// остальные попытки завершаются с NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task Uniqueness_Id_Test_For_Competitive_Requests()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            var bookingId = Guid.NewGuid();

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            _bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = CreateBookingService();

            var requestsCount = 10;

            // Act (действие)
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    return await service.CreateBookingAsync(eventId, userId);
                }));

            var results = await Task.WhenAll(tasks);

            // Assert (проверка)
            results.Should().HaveCount(requestsCount);

            results
                .Select(b => b.BookingId)
                .Should()
                .OnlyHaveUniqueItems();
        }

        /// <summary>
        /// Проверяет, что BookingProcessorService изменяет статус брони
        /// и устанавливает время обработки.
        /// </summary>
        [Fact]
        public async Task GetBookingStatus_ShouldReturnModifiedStatus_After_BookingProcessorService()
        {
            // Arrange (подготовка)
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test Event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 9
            };

            var fakeBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId, default))
                .ReturnsAsync(fakeBooking);

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId, default))
                .ReturnsAsync(fakeEvent);

            _unitOfWorkMock
                .Setup(r => r.SaveChangesAsync(default))
                .ReturnsAsync(1);

            var processor = new BookingProcessorService(
                _bookingRepositoryMock.Object, 
                _eventRepositoryMock.Object, 
                _unitOfWorkMock.Object, 
                NullLogger<BookingProcessorService>.Instance);

            // Act (действие)
            await processor.ProcessBookingAsync(bookingId);

            // Assert (проверка)
            fakeBooking.Status.Should().Be(BookingStatus.Confirmed);
            fakeBooking.ProcessedAt.Should().NotBeNull();
            fakeBooking.ProcessedAt.Should().BeAfter(fakeBooking.CreatedAt);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что лимиты активных броней разных пользователей
        /// не влияют друг на друга.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WhenAnotherUserReachedLimit_ShouldCreateBookingSuccessfully()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var firstUserId = Guid.NewGuid();
            var secondUserId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 15,
                AvailableSeats = 15
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            // У первого пользователя лимит достигнут.
            _bookingRepositoryMock
                .Setup(r => r.GetActiveBookingsCountAsync(firstUserId))
                .ReturnsAsync(10);

            // У второго пользователя активных броней нет.
            _bookingRepositoryMock
                .Setup(r => r.GetActiveBookingsCountAsync(secondUserId))
                .ReturnsAsync(0);

            var service = CreateBookingService();

            // Act
            var booking = await service.CreateBookingAsync(eventId, secondUserId);

            // Assert
            booking.Should().NotBeNull();

            _bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        #endregion

        #region Unsuccessful scenarios for BookingService

        /// <summary>
        /// Проверяет, что при запросе несуществующей брони
        /// выбрасывается NotFoundException.
        /// </summary>
        [Fact]
        public async Task GetBookingById_WithNonExistingId_ShouldThrow_NotFoundException()
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
                .ThrowAsync<NotFoundException>();
        }

        /// <summary>
        /// Проверяет, что при попытке создать бронь для несуществующего события
        /// выбрасывается NotFoundException.
        /// </summary>
        [Fact]
        public async Task AddBooking_WithNonExistingOrRemovedEvent_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = CreateBookingService();

            // Assert (проверка)
            await service
                .Invoking(s => s.CreateBookingAsync(eventId, userId))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        /// <summary>
        /// Проверяет, что при отсутствии доступных мест
        /// создание брони выбрасывает NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WithNonAvailableSeats_ShouldThrow_NoAvailableSeatsException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 10,
                AvailableSeats = 0
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Assert (проверка)
            await service
                .Invoking(s => s.CreateBookingAsync(eventId, userId))
                .Should()
                .ThrowAsync<NoAvailableSeatsException>();
        }

        /// <summary>
        /// Проверяет, что при попытке забронировать прошедшее или начавшееся событие
        /// выбрасывается EventAlreadyStartedException.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WhenEventAlreadyStarted_ShouldThrow_EventAlreadyStartedException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var startAt = default(DateTime);

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = startAt,
                EndAt = startAt.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 0
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Assert (проверка)
            await service
                .Invoking(s => s.CreateBookingAsync(eventId, userId))
                .Should()
                .ThrowAsync<EventAlreadyStartedException>();
        }

        /// <summary>
        /// Проверяет, что при достижении пользователем лимита активных броней
        /// создание следующей брони выбрасывает ActiveBookingLimitExceededException.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WhenBookingLimitIsReached_ShouldThrow_ActiveBookingLimitExceededException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 15,
                AvailableSeats = 15
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            _bookingRepositoryMock.Setup(
                r => r.GetActiveBookingsCountAsync(userId))
                .ReturnsAsync(10);

            var service = CreateBookingService();

            // Assert (проверка)
            await service
                .Invoking(s => s.CreateBookingAsync(eventId, userId))
                .Should()
                .ThrowAsync<ActiveBookingLimitExceededException>();
        }

        /// <summary>
        /// Проверяет, что при отмене брони со статусом Canceled 
        /// выбрасывает BadRequestException.
        /// </summary>
        [Fact]
        public async Task CancelBooking_With_BookingStatusCancelled_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Cancelled,
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            await service
                .Invoking(s => s.CancelBookingAsync(bookingId, userId, Role.User))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        /// <summary>
        /// Проверяет, что при отмене брони пользователем которому не принадлежит бронь
        /// выбрасывает BookingAccessDeniedException.
        /// </summary>
        [Fact]
        public async Task CancelBooking_ByUserWhoDoesNotOwnBooking_ShouldThrow_BookingAccessDeniedException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var notOwnUserId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Confirmed,
            };

            _bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateBookingService();

            // Act (действие)
            await service
                .Invoking(s => s.CancelBookingAsync(bookingId, notOwnUserId, Role.User))
                .Should()
                .ThrowAsync<BookingAccessDeniedException>();
        }

        #endregion
    }
}
