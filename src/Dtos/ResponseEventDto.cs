namespace EventManagementService.Dtos
{
    /// <summary>
    /// DTO событие для ответов
    /// </summary>
    public class ResponseEventDto
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название события.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        public DateTime? EndAt { get; set; }
    }
}
