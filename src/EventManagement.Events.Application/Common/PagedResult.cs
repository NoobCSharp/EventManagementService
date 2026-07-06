namespace EventManagement.Events.Infrastructure.Common
{
    public record PagedResult<T>
    {
        // Номер страницы
        public int Page { get; init; }

        // Количество элементов на странице
        public int PageSize { get; init; }

        // Общее количество элементов, удовлетворяющих фильтру (без учета пагинации)
        public int TotalCount { get; init; }

        // Коллекция элементов на текущей странице
        public IReadOnlyCollection<T> Items { get; init; } = [];
    }
}
