using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Consumers;
using EventManagement.Shared.Kafka.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventManagement.Shared.Kafka.Topics;

namespace EventManagement.Shared.Kafka.Extensions
{
    /// <summary>
    /// Методы расширения для регистрации Kafka Consumer компонентов в DI-контейнере
    /// Отвечает за настройку конфигурации, Consumer и вспомогательных сервисов
    /// </summary>
    public static class KafkaConsumerExtensions
    {
        /// <summary>
        /// Регистрирует Kafka Consumer инфраструктуру:
        /// - настройки Consumer
        /// - реализацию IKafkaConsumer
        /// - инициализатор топиков
        /// </summary>
        /// <param name="services">Коллекция сервисов DI</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <returns>Обновленная коллекция сервисов</returns>
        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaConsumerOptions>(configuration.GetSection(KafkaConsumerOptions.SectionName));

            // IKafkaConsumer создаётся как Transient,
            // чтобы каждый BackgroundService получал отдельный экземпляр
            // и мог безопасно управлять жизненным циклом (Dispose).
            services.AddTransient<IKafkaConsumer, KafkaConsumer>();

            // Инициализатор топиков — singleton, так как
            // он не хранит состояние и используется глобально при старте
            services.AddSingleton<IKafkaTopicInitializer, KafkaTopicInitializer>();

            return services;
        }
    }
}
