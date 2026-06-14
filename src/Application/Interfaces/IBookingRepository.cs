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

        /// <summary>
        /// Получает количество активных броней для указанного пользователя.
        /// </summary>
        /// <param name="userId">
        /// уникальный идентификатор пользователя, для которого необходимо получить количество активных броней.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Количество активных броней для указанного пользователя.
        /// </returns>
        Task<int> GetActiveBookingsCountAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет бронь из хранилища данных.
        /// </summary>
        /// <param name="booking">
        /// Бронь, которую необходимо удалить.
        /// </param>
        void RemoveBooking(Booking booking);
    }
}
