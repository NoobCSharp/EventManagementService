using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<EventService>();
            services.AddScoped<BookingService>();

            return services;
        }
    }
}
