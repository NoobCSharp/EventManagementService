using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public interface IBookingStore
    {
        /// <summary>
        /// Коллекция броней в хранилище
        /// </summary>
        public List<Booking> Bookings { get; set; }

        /// <summary>
        /// Получить все брони со статусом "Pending" и не имеющие дату обработки (ProcessedAt == null).
        /// </summary>
        /// <returns>Коллекция броней со статусом "Pending" и без даты обработки.</returns>
        IEnumerable<Booking> GetPending();

        /// <summary>
        /// Обновляет бронь новой информацией
        /// </summary>
        /// <param name="booking">Экземпляр брони, содержащий обновленные данные</param>
        /// <remarks>Логика зарезервирована для будущих разработок в текущей версии не используется</remarks>
        void Update(Booking booking);
    }
}
