namespace Domain.Enums
{
    /// <summary>
    /// Перечисление статуса брони
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Бронь создана, ожидает обработки
        /// </summary>
        Pending,

        /// <summary>
        /// Бронь подтверждена обработчиком
        /// </summary>
        Confirmed,

        /// <summary>
        /// Бронь отклонена обработчиком
        /// </summary>
        Rejected,

        /// <summary>
        /// Бронь отменена пользователем до обработки
        /// </summary>
        Cancelled
    }
}
