using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.Repositories;
using EventManagement.Events.Infrastructure.Security;
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

            var jwtOptions = new JwtValidationOptions
            {
                Secret = configuration["Jwt:Secret"]!,
                Issuer = configuration["Jwt:Issuer"]!,
                Audience = configuration["Jwt:Audience"]!
            };

            services.AddSingleton(jwtOptions);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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
