using Confluent.Kafka;

namespace EventManagement.Shared.Kafka.Options
{
    /// <summary>
    /// Настройки Kafka Producer, считываемые из конфигурации приложения.
    /// Определяют параметры подключения к брокеру и гарантии доставки сообщений.
    /// </summary>
    public sealed class KafkaProducerOptions
    {
        /// <summary>
        /// Имя секции конфигурации с настройками Producer
        /// </summary>
        public const string SectionName = "Kafka:Producer";

        /// <summary>
        /// Адрес Kafka-брокера
        /// </summary>
        public string BootstrapServers { get; set; } = "localhost:9092";

        /// <summary>
        /// Уровень подтверждения доставки сообщения
        /// Определяет, сколько реплик должно подтвердить запись
        /// перед тем, как отправка будет считаться успешной
        /// </summary>
        public Acks Acks { get; set; } = Acks.All;
    }
}
