namespace EventManagement.Events.Application.Dtos
{
    public record EventDtoResponse
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        public Guid EventId { get; init; }

        /// <summary>
        /// Название события.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        public DateTime StartAt { get; init; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        public DateTime EndAt { get; init; }

        /// <summary>
        /// Общее количество мест на событие.
        /// </summary>
        public int? TotalSeats { get; init; }

        /// <summary>
        /// Текущее количество свободных мест.
        /// </summary>
        public int? AvailableSeats { get; init; }
    }
}
