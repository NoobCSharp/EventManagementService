namespace EventManagementService.Filters
{
    public record EventFilter
    {
        /// <summary>
        /// Фильтр по названию события
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Фильтр по дате начала события
        /// </summary>
        public DateTime? From {  get; init; }

        /// <summary>
        /// Фильтр по дате окончания события
        /// </summary>
        public DateTime? To { get; init; }

        /// <summary>
        /// Текущая страница
        /// </summary>
        public int Page { get; init; } = 1;

        /// <summary>
        /// Количество элементов на странице
        /// </summary>
        public int PageSize { get; init; } = 10;
    }
}
