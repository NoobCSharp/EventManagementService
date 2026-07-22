using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Infrastructure.DataAccess;
using EventManagement.Identity.Infrastructure.Repositories;
using EventManagement.Identity.Infrastructure.Security;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;

namespace EventManagement.Identity.Infrastructure
{
    public static class InfrastructureInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(connectionString);                        // Обязательно
#if DEBUG
                options
                        .LogTo(Console.WriteLine, LogLevel.Information)     // Удобно в разработке
                        .EnableDetailedErrors()                             // Удобно в разработке
                        .EnableSensitiveDataLogging();                      // Осторожно! Только для dev
#endif
            });

            services.AddScoped<IUserRepository, UserRepository>();

            services.Configure<JwtGenerationOptions>(configuration.GetSection(JwtGenerationOptions.SectionName));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwt = configuration.GetSection(JwtGenerationOptions.SectionName).Get<JwtGenerationOptions>()!;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();

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
                    .AddService(serviceName: "identity-service"));

            return services;
        }
    }
}

