namespace EventManagement.Identity.Infrastructure.Security
{
    public sealed class JwtGenerationOptions
    {
        /// <summary>
        /// Имя секции конфигурации с настройками JWT.
        /// </summary>
        public const string SectionName = "Jwt";

        /// <summary>
        /// Секретный ключ, используемый для подписи JWT-токенов.
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Издатель JWT-токенов.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Получатель (аудитория), для которого предназначены JWT-токены.
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Время жизни JWT-токена в минутах.
        /// </summary>
        public int LifetimeMinutes { get; set; }
    }
}
