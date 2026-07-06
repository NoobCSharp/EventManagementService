using Confluent.Kafka;

namespace EventManagement.Shared.Kafka.Options
{
    /// <summary>
    /// Настройки Kafka Consumer, считываемые из конфигурации приложения
    /// Определяют параметры подключения к брокеру и поведения потребителя
    /// </summary>
    public sealed class KafkaConsumerOptions
    {
        /// <summary>
        /// Имя секции конфигурации с настройками Consumer
        /// </summary>
        public const string SectionName = "Kafka:Consumer";

        /// <summary>
        /// Адрес Kafka-брокера
        /// </summary>
        public string BootstrapServers { get; set; } = "localhost:9092";

        /// <summary>
        /// Имя группы потребителей
        /// Все Consumer с одинаковым ConsumerGroup совместно обрабатывают
        /// сообщения одного топика, разделяя между собой его партиции
        /// </summary>
        public string ConsumerGroup { get; set; } = string.Empty;

        /// <summary>
        /// Определяет, с какого места начинать чтение сообщений,
        /// если для группы потребителей отсутствует сохраненный offset
        /// </summary>
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;

        /// <summary>
        /// Включает или отключает автоматическое сохранение позиции
        /// обработанного сообщения. При значении <c>false</c> приложение
        /// самостоятельно управляет сохранением offset
        public bool EnableAutoOffsetStore { get; set; } = false;

        /// <summary>
        /// Включает или отключает автоматический commit offset
        /// При значении <c>false</c> подтверждение обработки сообщений
        /// выполняется вручную после успешной обработки
        /// </summary>
        public bool EnableAutoCommit { get; set; } = false;
    }
}
