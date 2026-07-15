namespace EventManagement.Events.Application.Caching
{
    /// <summary>
    /// Настройки времени жизни записей в кэше.
    /// </summary>
    public class CacheTtlOptions
    {
        /// <summary>
        /// Имя секции конфигурации с настройками Ttl
        /// </summary>
        public const string SectionName = "Redis:Ttl";

        /// <summary>
        /// Время жизни кэша события в минутах.
        /// </summary>
        public int EventMinutes { get; set; } = 1;

        /// <summary>
        /// Время жизни кэша списка десяти самых популярных событий в минутах.
        /// </summary>
        public int Top10EventsMinutes { get; set; } = 1;
    }
}
