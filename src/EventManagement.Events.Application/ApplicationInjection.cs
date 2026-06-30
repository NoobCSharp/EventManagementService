using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Events.Application
{
    public static class ApplicationInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
