using EventManagement.Shared.Kafka.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventManagement.Shared.Kafka.Abstraction
{
    /// <summary>
    /// Базовый BackgroundService для Kafka Consumer.
    /// Реализует цикл получения сообщений, их десериализацию и передачу в обработчик
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения, получаемого из Kafka</typeparam>
    public abstract class KafkaConsumerBackgroundService<TMessage> : BackgroundService
    {
        private readonly IKafkaConsumer _consumer;
        private readonly ILogger _logger;

        /// <summary>
        /// Базовый конструктор Kafka Consumer BackgroundService
        /// </summary>
        /// <param name="consumer">Kafka consumer для чтения сообщений</param>
        /// <param name="logger">Логгер для мониторинга работы сервиса</param>
        protected KafkaConsumerBackgroundService(IKafkaConsumer consumer, ILogger logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Kafka topic, который слушает данный consumer.
        /// </summary>
        protected abstract string Topic { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Основной цикл обработки сообщений Kafka
        /// Выполняет подписку, чтение сообщений, десериализацию и вызов handler'а
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer started. Topic: {Topic}", Topic);

            await _consumer.Subscribe(Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Блокирующий вызов: ждём следующего сообщения от брокера (pull-модель)
                    // При отмене CancellationToken бросает OperationCanceledException
                    var result = _consumer.Consume(stoppingToken);

                    if (result is null)
                        continue;

                    // Десериализация сообщения из JSON
                    var message = JsonSerializer.Deserialize<TMessage>(result.Message.Value);

                    if (message is null)
                    {
                        _logger.LogWarning("Invalid message for topic {Topic}: {Message}",
                            Topic,
                            result.Message.Value);

                        continue;
                    }

                    // Обработка бизнес-логики сообщения
                    await HandleAsync(message, stoppingToken);

                    // Сохраняем офсет в локальный буфер — не обращение к брокеру
                    // Консьюмер отправит накопленные офсеты в Kafka в фоне по таймеру
                    // При сбое до этой строки сообщение будет повторно доставлено (at-least-once)
                    // _consumer.StoreOffset(result);

                    // Commit offset после успешной обработки
                    // (гарантия at-least-once доставки)
                    // при падении до Commit сообщение придет повторно
                    _consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение работы при остановке хоста
                _logger.LogInformation("Consumer stopped");
            }
            finally
            {
                // Освобождение ресурсов Kafka consumer
                // Close() отправляет leave group — rebalance происходит немедленно,
                // без ожидания session.timeout.ms. Также коммитит буферизованные офсеты
                _consumer.Dispose();
            }
        }
    }
}