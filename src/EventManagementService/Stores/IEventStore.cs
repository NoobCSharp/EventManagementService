using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public interface IEventStore
    {
        /// <summary>
        /// Коллекция событий в хранилище
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// Обновляет событие новой информацией
        /// </summary>
        /// <param name="event">Экземпляр события, содержащий обновленные данные</param>
        /// <remarks>Логика зарезервирована для будущих разработок в текущей версии не используется</remarks>
        void Update(Event @event);
    }
}
