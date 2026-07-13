using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Options;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Bookings.Infrastructure.KafkaServices;
using EventManagement.Bookings.Infrastructure.Repositories;
using EventManagement.Bookings.Infrastructure.Security;
using EventManagement.Shared.Kafka.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManagement.Bookings.Infrastructure
{
    public static class InfrastructureInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            services.AddKafkaProducer(configuration);
            services.AddKafkaConsumer(configuration);

            services.AddHostedService<BookingCreatedConfirmedKafkaService>();
            services.AddHostedService<BookingCreatedFailedKafkaService>();

            services.AddHostedService<BookingCancelledConfirmedKafkaService>();
            services.AddHostedService<BookingCancelledFailedKafkaService>();

            services.AddDbContext<BookingsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);                        // Обязательно
#if DEBUG
                options
                        .LogTo(Console.WriteLine, LogLevel.Information)     // Удобно в разработке
                        .EnableDetailedErrors()                             // Удобно в разработке
                        .EnableSensitiveDataLogging();                      // Осторожно! Только для dev
#endif
            });

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.Configure<JwtValidationOptions>(configuration.GetSection(JwtValidationOptions.SectionName));

            var jwtOptions = configuration
                .GetSection(JwtValidationOptions.SectionName)
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

            services.Configure<BookingOptions>(configuration.GetSection("BookingOptions"));

            return services;
        }
    }
}
