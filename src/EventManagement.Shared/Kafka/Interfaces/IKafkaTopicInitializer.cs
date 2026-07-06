namespace EventManagement.Shared.Kafka.Interfaces
{
    /// <summary>
    /// Отвечает за инициализацию Kafka-топиков при запуске приложения
    /// Используется для автоматического создания необходимых топиков,
    /// если они отсутствуют в кластере Kafka
    /// </summary>
    public interface IKafkaTopicInitializer
    {
        /// <summary>
        /// Создает Kafka-топик, если он еще не существует
        /// Если топик уже существует, операция считается успешной
        /// </summary>
        /// <param name="topic">Имя Kafka-топика</param>
        /// <param name="partitions">Количество партиций в топике</param>
        /// <param name="replicationFactor">
        /// Фактор репликации (сколько копий данных хранится в кластере)
        /// </param>
        Task CreateTopicIfNotExistsAsync(string topic, int partitions = 1, short replicationFactor = 1);
    }
}