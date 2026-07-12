namespace EventManagement.Events.Infrastructure.Dtos
{
    public record EventDtoPaginatedResponse
    {
        /// <summary>
        /// Количество событий прошедших фильтрацию
        /// </summary>
        public int TotalEventsCount {  get; init; }

        /// <summary>
        /// Коллекция событий после фильтрации и пагинации
        /// </summary>
        public IReadOnlyList<EventDtoResponse> ResponseEventDtos { get; init; } = [];

        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int CurrentPage { get; init; }

        /// <summary>
        /// Количество элементов на текущей странице.
        /// </summary>
        public int NumberOnCurrentPage { get; init; }
    }
}
