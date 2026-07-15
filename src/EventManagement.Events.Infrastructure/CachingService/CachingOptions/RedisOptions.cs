namespace EventManagement.Events.Infrastructure.CachingService.CachingOptions
{
    /// <summary>
    /// Настройки подключения к Redis.
    /// Используются для конфигурирования клиента StackExchange.Redis
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Имя секции в файле конфигурации.
        /// </summary>
        public const string SectionName = "Redis";

        /// <summary>
        /// Адрес Redis-сервера в формате "host:port".
        /// </summary>
        public string RedisServers { get; set; } = "localhost:6379";

        /// <summary>
        /// Пароль для подключения к Redis.
        /// Оставляется пустым, если сервер не требует аутентификации.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Максимальное время (в миллисекундах), отведённое на установление
        /// соединения с Redis-сервером.
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// Максимальное время (в миллисекундах) ожидания выполнения
        /// синхронной операции Redis.
        /// </summary>
        public int SyncTimeout { get; set; } = 3000;

        /// <summary>
        /// Определяет поведение клиента при невозможности подключиться
        /// к Redis во время запуска приложения.
        /// Если значение false, клиент продолжит автоматически
        /// предпринимать попытки переподключения.
        /// </summary>
        public bool AbortOnConnectFail { get; set; } = false;

        /// <summary>
        /// Количество повторных попыток подключения к Redis
        /// при возникновении ошибки соединения.
        /// </summary>
        public int ConnectRetry { get; set; } = 3; 
    }
}
