namespace EventManagementService.Dtos
{
    /// <summary>
    /// DTO событие для ответов
    /// </summary>
    public record ResponseEventDto
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Название события.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        public DateTime? StartAt { get; init; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        public DateTime? EndAt { get; init; }
    }
}
