using Confluent.Kafka;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventManagement.Shared.Kafka.Producers
{
    /// <summary>
    /// Универсальный Producer для публикации сообщений в Kafka.
    /// Выполняет сериализацию сообщений, их отправку в указанный топик
    /// и ведет лог успешных и неудачных операций.
    /// </summary>
    public sealed class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        private readonly ILogger<KafkaProducer> _logger;

        /// <summary>
        /// Инициализирует Producer Kafka с указанными параметрами подключения
        /// </summary>
        /// <param name="options">Настройки Producer Kafka.</param>
        /// <param name="logger">Логгер для регистрации событий отправки.</param>
        public KafkaProducer(IOptions<KafkaProducerOptions> options, ILogger<KafkaProducer> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                Acks = options.Value.Acks,
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Публикует сообщение в указанный Kafka-топик
        /// Сообщение сериализуется в JSON. Если ключ не указан,
        /// автоматически генерируется уникальный идентификатор
        /// </summary>
        /// <typeparam name="TMessage">Тип отправляемого сообщения</typeparam>
        /// <param name="topic">Имя Kafka-топика</param>
        /// <param name="message">Сообщение для публикации</param>
        /// <param name="key">
        /// Ключ сообщения, используемый Kafka для распределения по партициям
        /// Если не указан, генерируется автоматически
        /// </param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        public async Task ProduceAsync<TMessage>(string topic, TMessage message, string? key = null, CancellationToken cancellationToken = default)
        {
            try
            {
                string json = JsonSerializer.Serialize(message);

                var kafkaMessage = new Message<string, string>
                {
                    Key = key ?? Guid.NewGuid().ToString(),
                    Value = json
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

                _logger.LogInformation(
                    "Kafka message sent. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}", 
                    result.Topic, 
                    result.Partition.Value, 
                    result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Kafka produce failed. Topic: {Topic}", topic);

                throw;
            }
        }

        /// <summary>
        /// Завершает работу Producer, дожидаясь отправки всех сообщений
        /// после чего освобождает используемые ресурсы
        /// </summary>
        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
