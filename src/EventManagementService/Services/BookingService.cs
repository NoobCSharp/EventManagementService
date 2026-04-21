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

        /// <summary>
        /// Объект для блокировки, используемый при модификации коллекции бронирований.
        /// Предназначен для обеспечения простейшей локальной синхронизации (монитор lock).
        /// Примечание: Необходимо применять <c>lock(_bookingLock)</c> вокруг критических разделов, 
        /// если множество потоков может одновременно изменять <c>_bookingStore.Bookings</c>.
        /// </summary>
        private readonly object _bookingLock = new();

        public BookingService(IEventStore eventStore, IBookingStore bookingStore)
        {
            _eventStore = eventStore;
            _bookingStore = bookingStore;
        }

        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// </summary>
        /// <param name="eventId">
        /// Уникальный идентификатор события, к которому относится бронь.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        /// <remarks>
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// Если нет доступных мест на событие, бросает исключение NoAvailableSeatsException
        /// </remarks>
        public Task<BookingDtoResponse> CreateBookingAsync(Guid eventId)
        {
            lock (_bookingLock)
            {
                var existingEvent = _eventStore.Events
                    .FirstOrDefault(e => e.EventId == eventId);

                if (existingEvent is null)
                    throw new NotFoundException("Событие по указанному идентификатору не найдено!");

                if (!existingEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException("Нет доступных мест для бронирования на данное событие!");

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = null
                };

                _bookingStore.Bookings.Add(booking);

                var bookingDtoResponse = BookingMapper.BookingToResponse(booking);

                return Task.FromResult(bookingDtoResponse);
            }
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
            var existingBooking = _bookingStore.Bookings.FirstOrDefault(b => b.Id == id);

            if (existingBooking is null)
                throw new NotFoundException("Бронирование по указанному идентификатору не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return Task.FromResult(bookingDtoResponse);
        }
    }
}
