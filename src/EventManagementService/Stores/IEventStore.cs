using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public interface IEventStore
    {
        /// <summary>
        /// Коллекция событий в хранилище
        /// </summary>
        public List<Event> Events { get; set; }
    }
}
