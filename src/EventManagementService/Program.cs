using EventManagementService.BackgroundServices;
using EventManagementService.DataAccess;
using EventManagementService.Middlewares.ExceptionMiddleware;
using EventManagementService.Repositories;
using EventManagementService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EventManagementService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            builder.Services.AddScoped<EventService>();
            builder.Services.AddScoped<BookingService>();

            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Конвертер enum для вывода читабельного статуса
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter());
            });

            builder.Services.AddHostedService<BookingProcessingService>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseNpgsql(connectionString)                      // Обязательно
            //           .LogTo(Console.WriteLine, LogLevel.Information)   // Удобно в разработке
            //           .EnableDetailedErrors()                           // Удобно в разработке
            //           .EnableSensitiveDataLogging());                   // Осторожно! Только для dev 


            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseExceptionHandlingMiddleware();
   
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();

                builder.Host.UseDefaultServiceProvider(options =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            app.MapControllers();
            app.Run();
        }
    }
}
