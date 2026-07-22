using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.CachingService;
using EventManagement.Events.Infrastructure.CachingService.CachingOptions;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.Interfaces;
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
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Text;

namespace EventManagement.Events.Infrastructure
{
    public static class InfrastructureInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddKafkaProducer(configuration);
            services.AddKafkaConsumer(configuration);

            services.AddHostedService<BookingCreatedKafkaService>();
            services.AddHostedService<BookingCancelledKafkaService>();

            services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

            var redisOptions = configuration
                .GetSection(RedisOptions.SectionName)
                .Get<RedisOptions>()!;

            var options = new ConfigurationOptions
            {
                Password = redisOptions.Password,
                ConnectTimeout = redisOptions.ConnectTimeout,
                SyncTimeout = redisOptions.SyncTimeout,
                AbortOnConnectFail = redisOptions.AbortOnConnectFail,
                ConnectRetry = redisOptions.ConnectRetry
            };

            options.EndPoints.Add(redisOptions.RedisServers);

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));

            services.AddScoped<ICacheService, RedisCacheService>();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            services.AddDbContext<EventsDbContext>(options =>
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

            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Исключаем системные запросы из трейсинга
                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path;

                            // Если запрос идёт на /health или /metrics, спан НЕ создаётся
                            return !path.StartsWithSegments("/health") && !path.StartsWithSegments("/metrics");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(o => o.Endpoint = new Uri(configuration["Otlp:Endpoint"]!)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter())
                .ConfigureResource(r => r
                    .AddService(serviceName: "events-service"));

            return services;
        }
    }
}
