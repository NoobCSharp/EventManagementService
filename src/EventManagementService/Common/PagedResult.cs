namespace EventManagementService.Entities
{
    public record PagedResult<T>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; init; }

        public IReadOnlyCollection<T> Items { get; init; } = [];
    }
}
