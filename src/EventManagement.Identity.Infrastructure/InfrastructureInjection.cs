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
using System.Text;

namespace EventManagement.Identity.Infrastructure
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

            services.AddScoped<IUserRepository, UserRepository>();

            var jwtOptions = new JwtGenerationOptions
            {
                Secret = configuration["Jwt:Secret"]!,
                Issuer = configuration["Jwt:Issuer"]!,
                Audience = configuration["Jwt:Audience"]!,
                LifetimeMinutes = int.Parse(configuration["Jwt:LifetimeMinutes"]!)
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

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();

            return services;
        }
    }
}

