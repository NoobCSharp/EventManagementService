using EventManagementService.Dtos.BookingDtos;
using EventManagementService.Enums;
using EventManagementService.Exceptions;
using EventManagementService.Mappers;
using EventManagementService.Models;
using EventManagementService.Stores;

namespace EventManagementService.Services
{
    public class BookingService : IBookingService
    {
        private readonly IEventStore _eventStore;
        private readonly IBookingStore _bookingStore;

        public BookingService(IEventStore eventStore, IBookingStore bookingStore)
        {
            _eventStore = eventStore;
            _bookingStore = bookingStore;
        }

        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="eventId">
        /// Уникальный идентификатор события, к которому относится бронь.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        public Task<BookingDtoResponse> CreateBookingAsync(Guid eventId)
        {
            var existingEvent = _eventStore.Events
                .FirstOrDefault(e => e.EvendId == eventId);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            _bookingStore.Bookings.Add(booking);

            var bookingDtoResponse = BookingMapper.BookingToResponse(booking);

            return Task.FromResult(bookingDtoResponse);
        }

        /// <summary>
        /// Получает бронь по Id.
        /// Если бронь не найдена, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <returns>
        /// Объект брони из коллекции.
        /// </returns>
        public Task<BookingDtoResponse> GetBookingByIdAsync(Guid id)
        {
            var existingBooking = _bookingStore.Bookings.FirstOrDefault(b => b.BookingId == id);

            if (existingBooking is null)
                throw new NotFoundException("Бронирование по указанному Id не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return Task.FromResult(bookingDtoResponse); 
        }
    }
}
