using EventManagement.Bookings.Application.Dtos;
using EventManagement.Bookings.Domain.Enums;

namespace EventManagement.Bookings.Application.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Получает бронь по Id.
        /// Если бронь не найдена, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="bookingId">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <returns>
        /// Объект брони из хранилища данных.
        /// </returns>
        Task<BookingDtoResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// </summary>
        /// <param name="eventId">
        /// Уникальный идентификатор события, к которому относится бронь.
        /// </param>
        /// <param name="userId">
        /// Уникальный идентификатор пользователя, к которому относится бронь.
        /// </param>
        /// /// <param name="seatCount">
        /// Количество мест для бронирования.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        /// <remarks>
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// Если событие уже началось или прошло, бросает исключение EventAlreadyStartedException.
        /// Если нет доступных мест на событие, бросает исключение NoAvailableSeatsException.
        /// Если пользователь превысил лимит активных бронирований, бросает исключение ActiveBookingLimitExceededException.
        /// </remarks>
        Task<BookingDtoResponse> CreateBookingAsync(Guid eventId, Guid userId, int seatCount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отменяет бронь по указанному идентификатору с учетом прав пользователя.
        /// </summary>
        /// <param name="bookingId">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <param name="userId">
        /// Уникальный идентификатор пользователя, выполняющего отмену.
        /// </param>
        /// <param name="role">
        /// Роль пользователя, выполняющего отмену.
        /// </param>
        /// <remarks>
        /// Если бронь не найдена, бросает исключение NotFoundException.
        /// Если пользователь пытается отменить чужую бронь без прав администратора,
        /// бросает исключение BookingAccessDeniedException.
        /// Если бронь уже отменена, бросает исключение BadRequestException.
        /// Если событие, связанное с бронью, уже началось, бросает исключение EventAlreadyStartedException.
        /// </remarks>
        Task CancelBookingAsync(Guid bookingId, Guid userId, Role role, CancellationToken cancellationToken = default);
    }
}