namespace EventManagementService.Dtos
{
    /// <summary>
    /// Объект возвращаемый результат пагинации
    /// </summary>
    public record PaginatedResultDto
    {
        /// <summary>
        /// Количество событий прошедших фильтрацию
        /// </summary>
        public int TotalEventsCount {  get; init; }

        /// <summary>
        /// Коллекция событий после фильтрации и пагинации
        /// </summary>
        public IEnumerable<ResponseEventDto>? ResponseEventDtos { get; init; } = [];

        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int CurrentPage { get; init; }

        /// <summary>
        /// Количество элементов на текущей странице.
        /// </summary>
        public int NumberEventsOnCurrentPage { get; init; }

    }
}
