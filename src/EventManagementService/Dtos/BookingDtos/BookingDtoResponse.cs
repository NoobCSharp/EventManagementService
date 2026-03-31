using EventManagementService.Enums;

namespace EventManagementService.Dtos.BookingDtos
{
    public record BookingDtoResponse
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь
        /// </summary>
        public Guid EventId { get; init; }

        /// <summary>
        /// Текущий статус брони
        /// </summary>
        public BookingStatus Status { get; init; }

        /// <summary>
        /// Дата и время создания брони
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Дата и время обработки
        /// </summary>
        public DateTime? ProcessedAt { get; init; }
    }
}
