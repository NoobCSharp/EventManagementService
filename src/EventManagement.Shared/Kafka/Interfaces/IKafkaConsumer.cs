using Confluent.Kafka;

namespace EventManagement.Shared.Kafka.Interfaces
{
    /// <summary>
    /// Представляет Kafka Consumer, отвечающий за подписку на топики,
    /// получение сообщений и управление подтверждением их обработки
    /// </summary>
    public interface IKafkaConsumer : IDisposable
    {
        /// <summary>
        /// Считывает следующее сообщение из подписанных топиков
        /// Возвращает <c>null</c>, если чтение не выполнено
        /// </summary>
        /// <returns>Полученное сообщение Kafka</returns>
        ConsumeResult<string, string>? Consume(CancellationToken cancellationToken = default);

        /// <summary>
        /// Подписывается на указанный Kafka-топик
        /// </summary>
        /// <param name="topic">Имя топика</param>
        Task Subscribe(string topic);

        /// <summary>
        /// Сохраняет позицию (offset) обработанного сообщения
        /// без немедленной отправки подтверждения брокеру
        /// </summary>
        /// <param name="result">Результат чтения сообщения</param>
        void StoreOffset(ConsumeResult<string, string> result);

        /// <summary>
        /// Подтверждает успешную обработку сообщения,
        /// фиксируя соответствующий offset в Kafka
        /// </summary>
        /// <param name="result">Результат чтения сообщения</param>
        void Commit(ConsumeResult<string, string> result);
    }
}