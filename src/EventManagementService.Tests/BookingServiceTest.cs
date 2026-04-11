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

        [Fact]
        public async Task AddBooking_ShouldAddBooking_To_Collection()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            var bookings = ServicesTestHelper.CreateBookings();

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
            var addedBooking = mockBookingStore.Object.Bookings.Single(e => e.Id == result.Id);
            // Проверяем, что бронь добавлена для события с указанным Id
            addedBooking.EventId.Should().Be(eventId);
            //Проверяем, что у добавленной брони установлен корректный статус при создании
            addedBooking.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task AddManyBooking_ShouldAddManyBooking_ToOneEvent()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            
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
            // Проверяем, что добавились только две брони, они имеют статус Pending, и принадлежат одному событию
            bookings.Should()
                .HaveCount(2)
                .And.OnlyContain(b => b.Status == BookingStatus.Pending)
                .And.OnlyContain(b => b.EventId == eventId);
            
            // Проверяем, что в коллекцию добавлены брони, проверяем по уникальному Id
            var addedBooking1 = mockBookingStore.Object.Bookings.Single(e => e.Id == bookingFirst.Id);
            var addedBooking2 = mockBookingStore.Object.Bookings.Single(e => e.Id == bookingLast.Id);
            

            // Проверяем, что BookingId уникальны
            addedBooking1.Id.Should().NotBe(addedBooking2.Id);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturn_Booking_From_Colletion_ById()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            var bookings = ServicesTestHelper.CreateBookings();

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
            result.Id.Should().Be(Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"));
        }

        [Fact]
        public async Task GetBookingStatus_ShouldReturnModifiedStatus_After_BookingProcessingService()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Id = Guid.NewGuid(),
                    Status = BookingStatus.Pending,
                    ProcessedAt = null
                }
            };

            var bookingStoreMock = new Mock<IBookingStore>();

            bookingStoreMock.Setup(s => s.Bookings).Returns(bookings);

            var services = new ServiceCollection();
            // Регистрируем мок как scoped, чтобы CreateScope() вернул его внутри scope
            services.AddScoped(_ => bookingStoreMock.Object);

            var provider = services.BuildServiceProvider();

            // Реализация ILogger, которая ничего не логирует (используется в тестах, чтобы не генерировать записи)
            var svc = new BookingProcessingService(provider, NullLogger<BookingProcessingService>.Instance);

            // Act
            await svc.ProcessPendingBookingsAsync(CancellationToken.None);

            // Assert
            bookings.First().Status.Should().Be(BookingStatus.Confirmed);
            bookings.First().ProcessedAt.Should().NotBeNull();
        }

        #endregion

        #region Unsuccessful scenarios for BookingService

        [Fact]
        public async Task GetBookingById_WithNonExistingId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            var bookings = ServicesTestHelper.CreateBookings();

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

        [Fact]
        public async Task AddBooking_WithNonExistingOrRemovedEvent_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            var bookings = ServicesTestHelper.CreateBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);
            mockBookingStore.Setup(b => b.Bookings).Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.CreateBookingAsync(Guid.Parse("EAD0E512-87E4-4825-98CF-8331D34C114F")))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        #endregion
    }
}
