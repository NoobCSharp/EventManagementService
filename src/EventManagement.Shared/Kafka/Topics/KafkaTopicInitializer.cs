using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Options;
using Microsoft.Extensions.Options;

namespace EventManagement.Shared.Kafka.Topics
{
    /// <summary>
    /// Создает Kafka-топики при запуске приложения.
    /// Если указанный топик уже существует, повторное создание игнорируется
    /// </summary>
    internal sealed class KafkaTopicInitializer : IKafkaTopicInitializer
    {
        private readonly KafkaProducerOptions _options;
        private readonly IAdminClient _admin;

        /// <summary>
        /// Инициализирует клиент администрирования Kafka,
        /// используемый для создания топиков
        /// </summary>
        /// <param name="options">Настройки подключения к Kafka.</param>
        public KafkaTopicInitializer(IOptions<KafkaProducerOptions> options)
        {
            _options = options.Value;

            var config = new AdminClientConfig
            {
                BootstrapServers = _options.BootstrapServers
            };

            _admin = new AdminClientBuilder(config).Build();
        }

        /// <summary>
        /// Создает топик, если он отсутствует в кластере Kafka.
        /// Если топик уже существует, исключение игнорируется
        /// </summary>
        /// <param name="topic">Имя создаваемого топика</param>
        /// <param name="partitions">Количество партиций. По умолчанию — 1</param>
        /// <param name="replicationFactor">Коэффициент репликации. По умолчанию — 1</param>
        public async Task CreateTopicIfNotExistsAsync(string topic, int partitions = 1, short replicationFactor = 1)
        {
            try
            {
                await _admin.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = partitions,
                        ReplicationFactor = replicationFactor
                    }
                });
            }
            catch (CreateTopicsException ex)
            {
                // Если топик уже существует — игнорируем.
                if (ex.Results.Any(r => r.Error.Code != ErrorCode.TopicAlreadyExists))
                    throw;
            }
        }
    }
}