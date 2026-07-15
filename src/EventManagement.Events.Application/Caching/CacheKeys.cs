namespace EventManagement.Events.Application.Caching
{
    /// <summary>
    /// Содержит методы для формирования ключей кэша.
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Возвращает ключ кэша для события по его идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор события.</param>
        public static string Event(Guid id) => $"event:{id}";

        /// <summary>
        /// Возвращает ключ кэша для списка десяти самых популярных событий.
        /// </summary>
        public static string Top10Events() => "events:top10";
    }
}
