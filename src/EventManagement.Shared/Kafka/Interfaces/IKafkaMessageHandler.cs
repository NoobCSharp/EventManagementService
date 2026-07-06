namespace EventManagement.Shared.Kafka.Interfaces
{
    /// <summary>
    /// Определяет обработчик сообщений Kafka определенного типа
    /// Реализует бизнес-логику, выполняемую после получения и
    /// десериализации сообщения Consumer
    /// </summary>
    /// <typeparam name="TMessage">Тип обрабатываемого сообщения</typeparam>
    public interface IKafkaMessageHandler<TMessage>
    {
        /// <summary>
        /// Выполняет обработку полученного сообщения
        /// </summary>
        /// <param name="message">Сообщение, полученное из Kafka</param>
        Task HandleAsync(TMessage message, CancellationToken ct);
    }
}