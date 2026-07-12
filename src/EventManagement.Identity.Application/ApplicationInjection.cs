using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Identity.Application
{
    public static class ApplicationInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
