using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Application
{
    public static class ApplicationInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();

            return services;
        }
    }
}
