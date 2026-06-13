using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBookingRepository
    {
        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// </summary>
        /// <param name="booking">
        /// Объект брони, который необходимо создать.
        /// </param>
        Task CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает бронь по идентификатору из хранилища данных.
        /// </summary>
        /// <param name="bookingId">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает все брони со статусом "Ожидает обработки" из хранилища данных.
        /// </summary>
        /// <returns>
        /// Коллекция объектов брони со статусом "Ожидает обработки".
        /// </returns>
        Task<IEnumerable<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
    }
}
