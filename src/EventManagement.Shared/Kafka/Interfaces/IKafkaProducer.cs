namespace EventManagement.Shared.Kafka.Interfaces
{
    /// <summary>
    /// Определяет контракт Kafka Producer, отвечающего за публикацию сообщений в указанные топики Kafka
    /// </summary>
    public interface IKafkaProducer : IDisposable
    {
        /// <summary>
        /// Отправляет сообщение в указанный Kafka-топик
        /// Сообщение сериализуется в строковый формат (обычно JSON)
        /// </summary>
        /// <typeparam name="TMessage">Тип отправляемого сообщения</typeparam>
        /// <param name="topic">Имя Kafka-топика</param>
        /// <param name="message">Сообщение для отправки</param>
        /// <param name="key">
        /// Ключ сообщения, используемый Kafka для распределения по партициям
        /// Если не указан, может быть сгенерирован автоматически
        /// </param>
        Task ProduceAsync<TMessage>(string topic, TMessage message, string? key = null, CancellationToken cancellationToken = default);
    }
}