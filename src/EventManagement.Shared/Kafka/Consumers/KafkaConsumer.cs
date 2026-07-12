using Confluent.Kafka;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManagement.Shared.Kafka.Consumers
{
    /// <summary>
    /// Универсальный Kafka Consumer для чтения сообщений из топиков
    /// Отвечает за подписку, получение сообщений и управление offset'ами
    /// </summary>
    public sealed class KafkaConsumer : IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IKafkaTopicInitializer _topicInitializer;
        private readonly ILogger<KafkaConsumer> _logger;

        /// <summary>
        /// Инициализирует Kafka Consumer с настройками подключения и логированием
        /// </summary>
        /// <param name="options">Конфигурация Consumer</param>
        /// <param name="topicInitializer">Сервис для создания топиков при необходимости</param>
        /// <param name="logger">Логгер для диагностики работы Consumer</param>
        public KafkaConsumer(IOptions<KafkaConsumerOptions> options, IKafkaTopicInitializer topicInitializer, ILogger<KafkaConsumer> logger)
        {
            _topicInitializer = topicInitializer;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.ConsumerGroup,
                AutoOffsetReset = options.Value.AutoOffsetReset,
                EnableAutoOffsetStore = options.Value.EnableAutoOffsetStore,
                EnableAutoCommit = options.Value.EnableAutoCommit
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Подписывается на указанный Kafka-топик
        /// Перед подпиской гарантирует, что топик существует
        /// </summary>
        /// <param name="topic">Имя Kafka-топика</param>
        public async Task Subscribe(string topic)
        {
            await _topicInitializer.CreateTopicIfNotExistsAsync(topic);

            _consumer.Subscribe(topic);

            _logger.LogInformation("Subscribed to topic {Topic}", topic);
        }

        /// <summary>
        /// Получает следующее сообщение из Kafka
        /// Метод является блокирующим до получения сообщения или отмены операции
        /// </summary>
        /// <returns>Полученное сообщение или null при ошибке</returns>
        public ConsumeResult<string, string>? Consume(CancellationToken cancellationToken = default)
        {
            try
            {
                return _consumer.Consume(cancellationToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
                throw;
            }
        }

        /// <summary>
        /// Сохраняет offset обработанного сообщения локально
        /// (без немедленного подтверждения брокеру)
        /// </summary>
        /// <param name="result">Результат получения сообщения</param>
        public void StoreOffset(ConsumeResult<string, string> result)
        {
            _consumer.StoreOffset(result);

            _logger.LogInformation("Offset stored: {TopicPartitionOffset}", result.TopicPartitionOffset);
        }

        /// <summary>
        /// Подтверждает обработку сообщения,
        /// фиксируя offset в Kafka (commit)
        /// </summary>
        /// <param name="result">Результат получения сообщения</param>
        public void Commit(ConsumeResult<string, string> result)
        {
            try
            {
                _consumer.Commit(result);
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Kafka commit error");
                throw;
            }
        }

        /// <summary>
        /// Корректно завершает работу Consumer:
        /// закрывает соединение с брокером и освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }
}
