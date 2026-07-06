using EventManagement.Bookings.Application;
using EventManagement.Bookings.Infrastructure;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagementService.Middlewares.ExceptionMiddleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace EventManagement.Bookings.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddEndpointsApiExplorer();

            // Add services to the container.
            // Add services to the container.
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                });

                options.AddSecurityRequirement(document =>
                {
                    var securityRequirement = new OpenApiSecurityRequirement();
                    var securitySchemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

                    securityRequirement.Add(securitySchemeReference, new List<string>());

                    return securityRequirement;
                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Регистрация сервисов приложения и репозиториев
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandlingMiddleware();

            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
