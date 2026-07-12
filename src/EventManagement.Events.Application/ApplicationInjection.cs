using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Events.Infrastructure
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
