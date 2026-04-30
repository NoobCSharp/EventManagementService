using EventManagementService.BackgroundServices;
using EventManagementService.DataAccess;
using EventManagementService.Enums;
using EventManagementService.Exceptions;
using EventManagementService.Models;
using EventManagementService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementService.Tests
{
    public class BookingServiceTestInMemory
    {
        private readonly ServiceProvider _serviceProvider;

        public BookingServiceTestInMemory()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Регистрируем сервисы
            services.AddScoped<EventService>();
            services.AddScoped<BookingService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        #region Successful scenarios for BookingService

        /// <summary>
        /// Проверяет, что при создании брони она добавляется в базу данных
        /// с корректным EventId и статусом Pending.
        /// </summary>
        [Fact]
        public async Task AddBooking_ShouldAddBooking_To_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var result = await bookingService.CreateBookingAsync(eventId);

            // Assert (проверка)
            // Проверяем, что в коллекцию добавлена одна бронь, проверяем по уникальному Id
            var addedBooking = context.Bookings.Single(e => e.BookingId == result.BookingId);
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var bookingFirst = await bookingService.CreateBookingAsync(eventId);
            var bookingLast = await bookingService.CreateBookingAsync(eventId);

            // Assert (проверка)
            // Проверяем, что добавились только две брони, они имеют статус Pending,
            // принадлежат одному событию и имеют уникальные Id
            context.Bookings.Should()
                .HaveCount(2)
                .And.OnlyContain(b => b.Status == BookingStatus.Pending)
                .And.OnlyContain(b => b.EventId == eventId)
                .And.OnlyHaveUniqueItems(b => b.BookingId);
        }

        /// <summary>
        /// Проверяет, что получение брони по Id возвращает корректный объект.
        /// </summary>
        [Fact]
        public async Task GetBookingById_ShouldReturn_Booking_From_DB_ById()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            // Act (действие)
            var booking = await bookingService.CreateBookingAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));
            var result = await bookingService.GetBookingByIdAsync(booking.BookingId);
            
            // Assert (проверка)
            // Проверяем, что бронь существует
            result.Should().NotBeNull();
            // Проверяем, что бронь имеет правильный Id
            result.BookingId.Should().Be(booking.BookingId);
        }

        /// <summary>
        /// Проверяет, что при создании брони количество доступных мест уменьшается на 1.
        /// </summary>
        [Fact]
        public async Task CreatingBooking_ShouldReduces_AvailableSeats_By_1()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            await bookingService.CreateBookingAsync(eventId);

            // Assert
            // Проверяем, что бронь успешно добавлена в базу данных и связана с правильным событием по Id
            context.Bookings.Should().HaveCount(1).And.OnlyContain(b => b.EventId == eventId);

            //Проверяем, что количество доступных мест уменьшилось на 1
            context.Events.Where(e => e.EventId == eventId).Single().AvailableSeats.Should().Be(99);
        }

        /// <summary>
        /// Проверяет, что при создании нескольких броней до лимита
        /// все операции успешны и каждая бронь имеет уникальный Id.
        /// </summary>
        [Fact]
        public async Task CreatingMultipleBookings_ToLimit_AllSuccessful_EachHasUniqueId()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
           
            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var limit = 5;
            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            var tasks = Enumerable.Range(0, limit)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

                    return await bookingService.CreateBookingAsync(eventId);
                }));

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что созданы несколько броней (до лимита) — все успешны,
            // у каждой уникальный Id
            context.Bookings.Should()
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("5F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException после исчерпания мест
            var tasks = Enumerable.Range(0, 2)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

                    try
                    {
                        await bookingService.CreateBookingAsync(eventId);
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
        }

        /// <summary>
        /// Проверяет, что Confirm устанавливает статус Confirmed
        /// и заполняет время обработки.
        /// </summary>
        [Fact]
        public async Task ConfirmBooking_Should_Set_StatusConfirmed_And_ProcessedAt()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var processedAt = DateTime.UtcNow;

            // Act
            var bookingDto = await bookingService.CreateBookingAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));
            var booking = context.Bookings.Single(b => b.BookingId == bookingDto.BookingId);
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
        public async Task RejectBooking_Should_Set_StatusReject_And_ProcessedAt()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var processedAt = DateTime.UtcNow;

            // Act
            var bookingDto = await bookingService.CreateBookingAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));
            var booking = context.Bookings.Single(b => b.BookingId == bookingDto.BookingId);
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            var bookingDto = await bookingService.CreateBookingAsync(eventId);
            var booking = context.Bookings.Single(b => b.BookingId == bookingDto.BookingId);

            booking.Reject(DateTime.UtcNow);
            context.Events.Single(e => e.EventId == eventId).ReleaseSeats();

            // Assert
            // Проверяем, что статус брони изменился на Rejected и количество доступных мест увеличилось на с 99 до 100
            context.Bookings.Single(b => b.BookingId == bookingDto.BookingId).Status.Should().Be(BookingStatus.Rejected);
            context.Events.Single(e => e.EventId == eventId).AvailableSeats.Should().Be(100);
        }

        /// <summary>
        /// Проверяет, что после отклонения брони и освобождения места
        /// можно создать новую бронь для того же события.
        /// </summary>
        [Fact]
        public async Task After_Reject_Should_Allow_New_Booking()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("5F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            var firstBookingDto = await bookingService.CreateBookingAsync(eventId);
            var firstBooking = context.Bookings.Single(b => b.BookingId == firstBookingDto.BookingId);

            firstBooking.Reject(DateTime.UtcNow);
            context.Events.Single(e => e.EventId == eventId).ReleaseSeats();

            var secondBookingDto = await bookingService.CreateBookingAsync(eventId);
            var secondBooking = context.Bookings.Single(b => b.BookingId == secondBookingDto.BookingId);

            // Assert
            // Проверяем, что после отклонения первой брони и освобождения места,
            // можно создать новую бронь для того же события, и у новой брони уникальный Id,
            // а количество доступных мест соответствует 0 (так как новое бронирование уже заняло место)
            secondBooking.Should().NotBeNull();
            secondBooking.BookingId.Should().NotBe(firstBooking.BookingId);
            context.Events.Single(e => e.EventId == eventId).AvailableSeats.Should().Be(0);
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var requestsCount = 20;
            var eventId = Guid.Parse("4F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

                    try
                    {
                        await bookingService.CreateBookingAsync(eventId);
                        return (Success: true, Exception: (Exception?)null);
                    }
                    catch (Exception ex)
                    {
                        return (Success: false, Exception: ex);
                    }
                }));

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что только 5 запросов были успешными, а остальные 15 вызвали NoAvailableSeatsException
            results.Count(r => r.Success).Should().Be(5);
            results.Count(r => r.Exception is NoAvailableSeatsException).Should().Be(15);
            // Проверяем, что количество доступных мест стало 0
            using var assertScope = _serviceProvider.CreateScope();
            var assertContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

            assertContext.Events.Single(e => e.EventId == eventId).AvailableSeats.Should().Be(0);
        }

        /// <summary>
        /// Проверяет защиту от переполнения (overbooking) при многопоточных запросах:
        /// количество успешных броней не превышает доступные места,
        /// остальные попытки завершаются с NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task Id_Uniqueness_Test_For_Competitive_Requests()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
           
            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var requestsCount = 10;
            var eventId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act
            var tasks = Enumerable.Range(0, requestsCount)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

                    return await bookingService.CreateBookingAsync(eventId);
                }));

            var results = await Task.WhenAll(tasks);

            // Assert
            // Проверяем, что созданы несколько броней (до лимита) — все успешны,
            // у каждой уникальный Id
            context.Bookings.Should()
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                EventId = eventId,
                Title = "Test Event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 10
            };

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                Event = @event,
                Status = BookingStatus.Pending
            };

            context.Events.Add(@event);
            context.Bookings.Add(booking);

            await context.SaveChangesAsync();

            var factory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var bookingProcessingService = new BookingProcessingService(factory, NullLogger<BookingProcessingService>.Instance);

            // Act
            await bookingProcessingService.ProcessBookingAsync(booking.BookingId);

            // Assert
            var updatedBooking = await context.Bookings.AsNoTracking().FirstAsync();
            updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
            updatedBooking.ProcessedAt.Should().NotBeNull();
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await bookingService.Invoking(s => s.GetBookingByIdAsync(Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF")))
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException для несуществующего события
            await bookingService.Invoking(s => s.CreateBookingAsync(Guid.Parse("EAD0E512-87E4-4825-98CF-8331D34C114F")))
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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NoAvailableSeatsException
            // при попытке создать бронь для события, у которого нет доступных мест
            await bookingService.Invoking(s => s.CreateBookingAsync(eventId))
                .Should()
                .ThrowAsync<NoAvailableSeatsException>();
        }

        #endregion
    }
}
