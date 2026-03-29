using EventManagementService.Middlewares.ExceptionMiddleware;
using EventManagementService.Services;
using EventManagementService.Stores;
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

            builder.Services.AddSingleton<IEventStore, EventStore>();
            builder.Services.AddSingleton<IBookingStore, BookingStore>();

            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IBookingService, BookingService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Конвертер enum для вывода читабельного статуса
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
            });

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
            app.MapControllers();
            app.Run();
        }
    }
}
