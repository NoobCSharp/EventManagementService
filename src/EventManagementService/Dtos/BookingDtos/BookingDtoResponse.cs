using EventManagementService.Enums;

namespace EventManagementService.Dtos.BookingDtos
{
    public record BookingDtoResponse
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        public Guid BookingId { get; init; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь
        /// </summary>
        public Guid EventId { get; init; }

        /// <summary>
        /// Текущий статус брони
        /// </summary>
        public BookingStatus Status { get; init; }
    }
}
