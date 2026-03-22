using EventManagementService.Models;

namespace EventManagementService.Repositories
{
    public interface IEventRepository
    {
        /// <summary>
        /// Коллекция событий в хранилище
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        ///  Получает следующий свободный Id.
        /// </summary>
        /// <returns>Id для создания события</returns>
        public int GetAvailableId();
    }
}
