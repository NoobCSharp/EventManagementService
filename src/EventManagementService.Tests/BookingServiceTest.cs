using EventManagementService.BackgroundServices;
using EventManagementService.Enums;
using EventManagementService.Exceptions;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementService.Stores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventManagementService.Tests
{
    public class BookingServiceTest
    {
        #region Successful scenarios for BookingService

        /// <summary>
        /// Проверяет, что при создании брони она добавляется в коллекцию
        /// с корректным EventId и статусом Pending.
        /// </summary>
        [Fact]
        public async Task AddBooking_ShouldAddBooking_To_Collection()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var bookings = ServicesTestHelper.CreateFakeBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var result = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            // Проверяем, что в коллекцию добавлена одна бронь, проверяем по уникальному Id
            var addedBooking = mockBookingStore.Object.Bookings.Single(e => e.BookingId == result.BookingId);
            // Проверяем, что бронь добавлена для события с указанным Id
            addedBooking.EventId.Should().Be(eventId);
            //Проверяем, что у добавленной брони установлен корректный статус при создании
            addedBooking.Status.Should().Be(BookingStatus.Pending);
        }

        /// <summary>
        /// Проверяет, что можно создать несколько броней для одного события,
        /// и каждая имеет уникальный Id и статус Pending.
        /// </summary>
        [Fact]
        public async Task AddManyBooking_ShouldAddManyBooking_ToOneEvent()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();

            // Начинаем с пустой коллекции
            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var bookingFirst = await service.CreateBookingAsync(eventId);
            var bookingLast = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            // Проверяем, что добавились только две брони, они имеют статус Pending,
            // принадлежат одному событию и имеют уникальные Id
            bookings.Should()
                .HaveCount(2)
                .And.OnlyContain(b => b.Status == BookingStatus.Pending)
                .And.OnlyContain(b => b.EventId == eventId)
                .And.OnlyHaveUniqueItems(b => b.BookingId);
        }

        /// <summary>
        /// Проверяет, что получение брони по Id возвращает корректный объект.
        /// </summary>
        [Fact]
        public async Task GetBookingById_ShouldReturn_Booking_From_Colletion_ById()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var bookings = ServicesTestHelper.CreateFakeBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act (действие)
            var result = await service.GetBookingByIdAsync(Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"));

            // Assert (проверка)
            // Проверяем, что бронь существует
            result.Should().NotBeNull();
            // Проверяем, что бронь имеет правильный Id
            result.BookingId.Should().Be(Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"));
        }

        /// <summary>
        /// Проверяет, что при создании брони количество доступных мест уменьшается на 1.
        /// </summary>
        [Fact]
        public async Task CreatingBooking_ShouldReduces_AvailableSeats_By_1()
        {
            // Arrange (подготовка)
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var events = new List<Event>
            {
                new Event
                {
                    EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"),
                    Title = "TestEvent",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 1
                }
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            await service.CreateBookingAsync(eventId);

            // Assert
            // Проверяем, что количество доступных мест уменьшилось на 1
            events.First(e => e.EventId == eventId).AvailableSeats.Should().Be(0);
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
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var events = new List<Event>
            {
                new Event
                {
                    EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"),
                    Title = "TestEvent",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 5,
                    AvailableSeats = 5
                }
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            var tasks = Enumerable.Range(0, limit)
                .Select(async _ =>
                    {
                        return await service.CreateBookingAsync(eventId);
                    });

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что созданы несколько броней (до лимита) — все успешны,
            // у каждой уникальный Id
            bookings.Should()
                .HaveCount(limit)
                .And.OnlyContain(b => b.EventId == eventId)
                .And.OnlyContain(b => b.Status == BookingStatus.Pending)
                .And.OnlyHaveUniqueItems(b => b.BookingId);
        }

        /// <summary>
        /// Проверяет, что после исчерпания доступных мест
        /// следующая попытка бронирования выбрасывает NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_AfterRunning_OutOfPlaces_NextAttempt_ShouldThrow_NoAvailableSeatsException()
        {
            // Arrange (подготовка)
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var events = new List<Event>
            {
                new Event
                {
                    EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"),
                    Title = "TestEvent",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 1
                }
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException после исчерпания мест
            var tasks = Enumerable.Range(0, 2)
                .Select(async _ =>
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
                });

            var results = await Task.WhenAll(tasks);

            results.Should().ContainSingle(r => r.Success);
            results.Should().ContainSingle(r => r.Exception is NoAvailableSeatsException);
        }

        /// <summary>
        /// Проверяет, что Confirm устанавливает статус Confirmed
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public void ConfirmBooking_Should_Set_StatusConfirmed_And_ProcessedAt()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var processedAt = DateTime.UtcNow;

            // Act
            booking.Confirm(processedAt);

            // Assert
            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.ProcessedAt.Should().Be(processedAt);
        }

        /// <summary>
        /// Проверяет, что Reject устанавливает статус Rejected
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public void RejectBooking_Should_Set_StatusReject_And_ProcessedAt()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var processedAt = DateTime.UtcNow;

            // Act
            booking.Reject(processedAt);

            // Assert
            // Проверяем, что статус брони изменился на Rejected и установлено время обработки
            booking.Status.Should().Be(BookingStatus.Rejected);
            booking.ProcessedAt.Should().Be(processedAt);
        }

        /// <summary>
        /// Проверяет, что при отклонении брони и освобождении места
        /// количество доступных мест увеличивается.
        /// </summary>
        [Fact]
        public async Task RejectBooking_Should_Release_AvailableSeats()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                EventId = eventId,
                Title = "TestEvent",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var events = new List<Event>()
            {
                @event
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            var bookingDto = await service.CreateBookingAsync(eventId);
            var booking = mockBookingStore.Object.Bookings.Single(b => b.BookingId == bookingDto.BookingId);

            booking.Reject(DateTime.UtcNow);
            @event.ReleaseSeats();

            // Assert
            // Проверяем, что статус брони изменился на Rejected и количество доступных мест увеличилось на 1
            booking.Status.Should().Be(BookingStatus.Rejected);
            @event.AvailableSeats.Should().Be(1);
        }

        /// <summary>
        /// Проверяет, что после отклонения брони и освобождения места
        /// можно создать новую бронь для того же события.
        /// </summary>
        [Fact]
        public async Task After_Reject_Should_Allow_New_Booking()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                EventId = eventId,
                Title = "TestEvent",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1
            };

            var events = new List<Event>()
            {
                @event
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            var firstBookingDto = await service.CreateBookingAsync(eventId);
            var firstBooking = mockBookingStore.Object.Bookings.Single(b => b.BookingId == firstBookingDto.BookingId);

            firstBooking.Reject(DateTime.UtcNow);
            @event.ReleaseSeats();

            var secondBookingDto = await service.CreateBookingAsync(eventId);
            var secondBooking = mockBookingStore.Object.Bookings.Single(b => b.BookingId == secondBookingDto.BookingId);

            // Assert
            // Проверяем, что после отклонения первой брони и освобождения места,
            // можно создать новую бронь для того же события, и у новой брони уникальный Id,
            // а количество доступных мест соответствует 0 (так как новое бронирование уже заняло место)
            secondBooking.Should().NotBeNull();
            secondBooking.BookingId.Should().NotBe(firstBooking.BookingId);
            @event.AvailableSeats.Should().Be(0);
        }

        /// <summary>
        /// Проверяет защиту от переполнения (overbooking) при конкурентных запросах:
        /// количество успешных бронирований не превышает доступные места,
        /// остальные попытки завершаются с NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task Overbooking_Protection_Test()
        {
            // Arrange (подготовка)
            var requestsCount = 20;
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var @event = new Event
            {
                EventId = eventId,
                Title = "TestEvent",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 5,
                AvailableSeats = 5
            };

            var events = new List<Event>
            {
                @event
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(async _ =>
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
                });

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что только 5 запросов были успешными, а остальные 15 вызвали NoAvailableSeatsException
            results.Count(r => r.Success).Should().Be(5);
            results.Count(r => r.Exception is NoAvailableSeatsException).Should().Be(15);
            // Проверяем, что количество доступных мест стало 0
            @event.AvailableSeats.Should().Be(0);
        }

        /// <summary>
        /// Проверяет защиту от переполнения (overbooking) при конкурентных запросах:
        /// количество успешных бронирований не превышает доступные места,
        /// остальные попытки завершаются с NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task Id_Uniqueness_Test_For_Competitive_Requests()
        {
            // Arrange (подготовка)
            var requestsCount = 10;
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var @event = new Event
            {
                EventId = eventId,
                Title = "TestEvent",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            var events = new List<Event>
            {
                @event
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(async _ =>
                {
                    return await service.CreateBookingAsync(eventId);
                });

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что созданы несколько броней (до лимита) — все успешны,
            // у каждой уникальный Id
            bookings.Should()
                .HaveCount(requestsCount)
                .And.OnlyHaveUniqueItems(b => b.BookingId);
        }

        /// <summary>
        /// Проверяет, что BookingProcessingService изменяет статус брони
        /// и устанавливает время обработки.
        /// </summary>
        [Fact]
        public async Task GetBookingStatus_ShouldReturnModifiedStatus_After_BookingProcessingService()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingId = Guid.NewGuid(),
                    EventId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF"),
                    Status = BookingStatus.Pending,
                    ProcessedAt = null
                }
            };

            var mockBookingStore = new Mock<IBookingStore>();
            var mockEventStore = new Mock<IEventStore>();

            mockBookingStore.Setup(s => s.Bookings).Returns(bookings);
            mockEventStore.Setup(e => e.Events).Returns(ServicesTestHelper.CreateFakeEvents());

            var services = new ServiceCollection();

            // Регистрируем мок как scoped, чтобы CreateScope() вернул его внутри scope
            services
                .AddScoped(_ => mockBookingStore.Object)
                .AddScoped(_ => mockEventStore.Object);

            var provider = services.BuildServiceProvider();

            // Реализация ILogger, которая ничего не логирует (используется в тестах, чтобы не генерировать записи)
            var bookingProcessingService = new BookingProcessingService(provider, NullLogger<BookingProcessingService>.Instance);

            // Act
            await bookingProcessingService.ProcessBookingAsync(bookings.First(), CancellationToken.None);

            // Assert
            bookings.First().Status.Should().Be(BookingStatus.Confirmed);
            bookings.First().ProcessedAt.Should().NotBeNull();
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
            var events = ServicesTestHelper.CreateFakeEvents();
            var bookings = ServicesTestHelper.CreateFakeBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.GetBookingByIdAsync(Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF")))
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
            var events = ServicesTestHelper.CreateFakeEvents();
            var bookings = ServicesTestHelper.CreateFakeBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException для несуществующего события
            await service.Invoking(s => s.CreateBookingAsync(Guid.Parse("EAD0E512-87E4-4825-98CF-8331D34C114F")))
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
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var events = new List<Event>
            {
                new Event
                {
                    EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"),
                    Title = "TestEvent",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 0
                }
            };

            var bookings = new List<Booking>();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException при попытке создать бронь для события, у которого нет доступных мест
            await service.Invoking(s => s.CreateBookingAsync(eventId))
                .Should()
                .ThrowAsync<NoAvailableSeatsException>();
        }

        #endregion
    }
}
