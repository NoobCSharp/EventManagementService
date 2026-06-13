namespace Application.Dtos.EventDtos
{
    public record EventDtoRequest
    {
        /// <summary>
        /// Название события
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Описание события
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата начала события
        /// </summary>
        public required DateTime StartAt { get; init; }

        /// <summary>
        /// Дата окончания события
        /// </summary>
        public required DateTime EndAt { get; init; }

        /// <summary>
        /// Общее количество мест на событие
        /// </summary>
        public required int TotalSeats { get; init; }
    }
}
