namespace EventManagement.Bookings.Domain.Enums
{
    /// <summary>
    /// Перечисление статуса брони
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Бронь создана
        /// </summary>
        Pending,

        /// <summary>
        /// Бронь подтверждена
        /// </summary>
        Confirmed,

        /// <summary>
        /// Бронь отклонена
        /// </summary>
        Rejected,

        /// <summary>
        /// Бронь отменена
        /// </summary>
        Cancelled
    }
}
