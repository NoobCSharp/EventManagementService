using EventManagementService.Dtos.BookingDtos;

namespace EventManagementService.Services
{
    public interface IBookingService
    {
        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// </summary>
        /// <param name="eventId">
        /// Уникальный идентификатор события, к которому относится бронь.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        Task<BookingDtoResponse> CreateBookingAsync(Guid eventId);

        /// <summary>
        /// Получает бронь по идентификатору
        /// </summary>
        /// <param name="bookingId">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <returns>
        /// Объект брони из коллекции.
        /// </returns>
        Task<BookingDtoResponse> GetBookingByIdAsync(Guid bookingId);
    }
}
