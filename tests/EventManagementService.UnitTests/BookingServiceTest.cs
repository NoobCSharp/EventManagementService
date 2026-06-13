using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventManagementService.UnitTests
{
    public class BookingServiceTest
    {
        #region Successful scenarios for BookingService

        /// <summary>
        /// Проверяет, что при создании брони она имеет корректные данные
        /// статуса и привязана к конкретному событию по Id
        /// </summary>
        [Fact]
        public async Task AddBooking_Should_AddBooking()
        {
            // Arrange (подготовка)
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var response = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            response.Should().NotBeNull();

            response.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            response.EventId.Should().Be(eventId);

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.Is<Booking>(
                        b => b.EventId == eventId &&
                        b.Status == BookingStatus.Pending)),
                Times.Once);

            unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        /// <summary>
        /// Проверяет, что можно создать несколько броней для одного события,
        /// и каждая имеет уникальный Id и статус Pending.
        /// </summary>
        [Fact]
        public async Task AddManyBooking_ShouldAdd_ManyBooking_ToOneEvent()
        {
            // Arrange (подготовка)
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var bookingFirst = await service.CreateBookingAsync(eventId);
            var bookingLast = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            bookingFirst.EventId.Should().Be(eventId);
            bookingLast.EventId.Should().Be(eventId);

            bookingFirst.Status.Should().Be(BookingStatus.Pending);
            bookingLast.Status.Should().Be(BookingStatus.Pending);

            bookingFirst.BookingId.Should().NotBe(bookingLast.BookingId);

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.Is<Booking>(b =>
                        b.EventId == eventId &&
                        b.Status == BookingStatus.Pending)),
                Times.Exactly(2));

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

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
                 Event = fakeEvent,
                 CreatedAt = DateTime.UtcNow,
                 ProcessedAt = DateTime.UtcNow,
                 Status = BookingStatus.Pending,
            };

            bookingRepositoryMock.Setup(
                r => r.GetBookingByIdAsync(bookingId)).
                ReturnsAsync(fakeBooking);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var response = await service.GetBookingByIdAsync(bookingId);

            // Assert (проверка)
            response.Should().NotBeNull();

            response.BookingId.Should().Be(bookingId);
            response.EventId.Should().Be(eventId);
            response.Status.Should().Be(BookingStatus.Pending);

            bookingRepositoryMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var response = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            fakeEvent.AvailableSeats.Should().Be(9);

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(
                    It.IsAny<Booking>()),
                Times.Once);

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var limit = 5;

            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var tasks = Enumerable.Range(0, limit)
                .Select(_ => Task.Run(async () =>
                {
                    return await service.CreateBookingAsync(eventId);
                }));

            var results = await Task.WhenAll(tasks);

            // Assert (проверка)
            results.Should().HaveCount(limit);

            results.Should()
                .OnlyContain(b =>
                    b.EventId == eventId &&
                    b.Status == BookingStatus.Pending);

            results
                .Select(b => b.BookingId)
                .Should()
                .OnlyHaveUniqueItems();

            fakeEvent.AvailableSeats.Should().Be(5);

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(limit));

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 2,
                AvailableSeats = 1
            };

            eventRepositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException после исчерпания мест
            var tasks = Enumerable.Range(0, 2)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await service.CreateBookingAsync(eventId);
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

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Once);

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

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
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            booking.Reject(DateTime.UtcNow);

            fakeEvent.ReleaseSeats();

            // Assert (проверка)
            booking.Status.Should().Be(BookingStatus.Rejected);

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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var bookingId = Guid.NewGuid();

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var firstResult = await service.CreateBookingAsync(eventId);

            fakeEvent.ReleaseSeats();

            var secondResult = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            firstResult.Should().NotBeNull();
            secondResult.Should().NotBeNull();

            firstResult.BookingId.Should().NotBe(secondResult.BookingId);

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(2));

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 5,
                AvailableSeats = 5
            };

            var bookingId = Guid.NewGuid();

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            var requestsCount = 20;

            // Act (действие)
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await service.CreateBookingAsync(eventId);
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

            bookingRepositoryMock.Verify(
                r => r.CreateBookingAsync(It.IsAny<Booking>()),
                Times.Exactly(5));

            unitOfWorkMock.Verify(
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

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

            var firstBooking = new Booking
            {
                BookingId = bookingId,
                EventId = eventId,
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
            };

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            bookingRepositoryMock
                .Setup(r => r.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var service = new BookingService(eventRepositoryMock.Object, bookingRepositoryMock.Object, unitOfWorkMock.Object);

            var requestsCount = 10;

            // Act (действие)
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    return await service.CreateBookingAsync(eventId);
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
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var eventRepositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

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
                Event = fakeEvent,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending
            };

            bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId, default))
                .ReturnsAsync(fakeBooking);

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId, default))
                .ReturnsAsync(fakeEvent);

            unitOfWorkMock
                .Setup(r => r.SaveChangesAsync(default))
                .ReturnsAsync(1);

            var processor = new BookingProcessorService(bookingRepositoryMock.Object, eventRepositoryMock.Object, unitOfWorkMock.Object, NullLogger<BookingProcessorService>.Instance);

            // Act (действие)
            await processor.ProcessBookingAsync(bookingId);

            // Assert (проверка)
            fakeBooking.Status.Should().Be(BookingStatus.Confirmed);
            fakeBooking.ProcessedAt.Should().NotBeNull();
            fakeBooking.ProcessedAt.Should().BeAfter(fakeBooking.CreatedAt);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var bookingId = Guid.NewGuid();

            bookingRepositoryMock
                .Setup(r => r.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking?)null);

            var bookingService = new BookingService(
                eventRepositoryMock.Object,
                bookingRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Assert (проверка)
            await bookingService
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var bookingService = new BookingService(
                eventRepositoryMock.Object,
                bookingRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Assert (проверка)
            await bookingService
                .Invoking(s => s.CreateBookingAsync(eventId))
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
            var eventRepositoryMock = new Mock<IEventRepository>();
            var bookingRepositoryMock = new Mock<IBookingRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Test event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 0
            };

            eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var bookingService = new BookingService(
                eventRepositoryMock.Object,
                bookingRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Assert (проверка)
            await bookingService
                .Invoking(s => s.CreateBookingAsync(eventId))
                .Should()
                .ThrowAsync<NoAvailableSeatsException>();
        }

        #endregion
    }
}
