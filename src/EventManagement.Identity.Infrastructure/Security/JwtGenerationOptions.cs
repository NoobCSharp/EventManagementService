namespace EventManagement.Identity.Infrastructure.Security
{
    public sealed class JwtGenerationOptions
    {
        public string Secret { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int LifetimeMinutes { get; set; }
    }
}
