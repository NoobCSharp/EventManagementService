using EventManagement.Events.Application.Caching;
using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Events.Infrastructure
{
    public static class ApplicationInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<CacheTtlOptions>(configuration.GetSection(CacheTtlOptions.SectionName));

            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
