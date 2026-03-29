using EventManagementService.Enums;
using EventManagementService.Exceptions;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementService.Stores;
using FluentAssertions;
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

            mockEventStore
                .Setup(e => e.Events)
                .Returns(events);

            mockBookingStore
                .Setup(b => b.Bookings)
                .Returns(bookings);

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

        [Fact]
        public async Task AddManyBooking_ShouldAddManyBooking_ToOneEvent()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            // Начинаем с пустого списка
            var bookings = new List<Booking>(); 

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore
                .Setup(e => e.Events)
                .Returns(events);

            mockBookingStore
                .Setup(b => b.Bookings)
                .Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var result1 = await service.CreateBookingAsync(eventId);
            var result2 = await service.CreateBookingAsync(eventId);

            // Assert (проверка)
            // Проверяем, что добавились только две брони
            bookings.Should().HaveCount(2);
            // Проверяем, что в коллекцию добавлены брони, проверяем по уникальному Id
            var addedBooking1 = mockBookingStore.Object.Bookings.Single(e => e.BookingId == result1.BookingId);
            var addedBooking2 = mockBookingStore.Object.Bookings.Single(e => e.BookingId == result2.BookingId);
            // Проверяем, что у добавленных броней установлен корректный статус при создании
            addedBooking1.Status.Should().Be(BookingStatus.Pending);
            addedBooking2.Status.Should().Be(BookingStatus.Pending);
            // Проверяем, что у добавленные брони имеют одинаковый Id события 
            addedBooking1.EventId.Should().Be(eventId);
            addedBooking2.EventId.Should().Be(eventId);
            // Проверяем, что BookingId уникальны
            addedBooking1.BookingId.Should().NotBe(addedBooking2.BookingId);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturn_Booking_From_Colletion_ById()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateEvents();
            var bookings = ServicesTestHelper.CreateBookings();

            var mockEventStore = new Mock<IEventStore>();
            var mockBookingStore = new Mock<IBookingStore>();

            mockEventStore
                .Setup(e => e.Events)
                .Returns(events);

            mockBookingStore
                .Setup(b => b.Bookings)
                .Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Act (действие)
            var result = await service.GetBookingByIdAsync(Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"));

            // Assert (проверка)
            // Проверяем, что бронь существует
            result.Should().NotBeNull();
            // Проверяем, что бронь имеет правильный Id
            result.BookingId.Should().Be(Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"));
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

            mockEventStore
                .Setup(e => e.Events)
                .Returns(events);

            mockBookingStore
                .Setup(b => b.Bookings)
                .Returns(bookings);

            var service = new BookingService(mockEventStore.Object, mockBookingStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.GetBookingByIdAsync(Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF")))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        #endregion
    }
}
