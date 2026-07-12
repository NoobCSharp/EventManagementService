namespace EventManagement.Shared.Kafka.Topics
{
    /// <summary>
    /// Содержит имена Kafka-топиков, используемых для обмена сообщениями между микросервисами.
    /// </summary>
    public static class KafkaTopics
    {
        /// <summary>
        /// Событие о создании новой брони. Публикуется сервисом Bookings
        /// </summary>
        public const string BookingCreated = "booking.created";

        /// <summary>
        /// Событие об успешном подтверждении бронирования
        /// </summary>
        public const string BookingCreatedConfirmed = "booking.created.confirmed";

        /// <summary>
        /// Событие о невозможности подтвердить бронирование
        /// </summary>
        public const string BookingCreatedFailed = "booking.created.failed";

        /// <summary>
        /// Событие о запросе на отмену бронирования
        /// </summary>
        public const string BookingCancelled = "booking.cancelled";

        /// <summary>
        /// Событие об успешной отмене бронирования
        /// </summary>
        public const string BookingCancelledConfirmed = "booking.cancelled.confirmed";

        // <summary>
        /// Событие о неудачной попытке отмены бронирования
        /// </summary>
        public const string BookingCancelledFailed = "booking.cancelled.failed";
    }
}