using Application;
using EventManagementService.BackgroundServices;
using EventManagementService.Middlewares.ExceptionMiddleware;
using Infrastructure;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EventManagementService
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

            // Конвертер enum для вывода читабельного статуса
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi();

            // Регистрация сервисов приложения и репозиториев
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Регистрация фоновой службы для обработки броней
            builder.Services.AddHostedService<BookingProcessorHostedService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandlingMiddleware();
            app.UseHttpsRedirection();

            app.UseAuthentication();
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
