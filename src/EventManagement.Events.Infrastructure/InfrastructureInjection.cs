using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.KafkaServices;
using EventManagement.Events.Infrastructure.Repositories;
using EventManagement.Events.Infrastructure.Security;
using EventManagement.Shared.Kafka.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManagement.Events.Infrastructure
{
    public static class InfrastructureInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            services.AddKafkaProducer(configuration);
            services.AddKafkaConsumer(configuration);

            services.AddHostedService<BookingCreatedKafkaService>();
            services.AddHostedService<BookingCancelledKafkaService>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);                        // Обязательно
#if DEBUG
                options
                        .LogTo(Console.WriteLine, LogLevel.Information)     // Удобно в разработке
                        .EnableDetailedErrors()                             // Удобно в разработке
                        .EnableSensitiveDataLogging();                      // Осторожно! Только для dev
#endif
            });

            services.AddScoped<IEventRepository, EventRepository>();

            services.Configure<JwtValidationOptions>(configuration.GetSection("Jwt"));

            var jwtOptions = configuration
                .GetSection("Jwt")
                .Get<JwtValidationOptions>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
