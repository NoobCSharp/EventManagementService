using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Options;
using EventManagement.Shared.Kafka.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Shared.Kafka.Extensions
{
    public static class KafkaProducerExtensions
    {
        /// <summary>
        /// Методы расширения для регистрации Kafka Producer компонентов в DI-контейнере
        /// Отвечает за настройку конфигурации Producer и его регистрацию в приложении
        /// </summary>
        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
        {
            /// <summary>
            /// Регистрирует Kafka Producer инфраструктуру:
            /// - настройки Producer
            /// - реализацию IKafkaProducer
            /// </summary>
            /// <param name="services">Коллекция сервисов DI</param>
            /// <param name="configuration">Конфигурация приложения</param>
            /// <returns>Обновленная коллекция сервисов</returns>
            services.Configure<KafkaProducerOptions>(configuration.GetSection(KafkaProducerOptions.SectionName));

            // Producer регистрируется как Singleton,
            // так как внутри используется потокобезопасный IProducer
            // и его рекомендуется переиспользовать в течение жизни приложения
            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            return services;
        }
    }
}
