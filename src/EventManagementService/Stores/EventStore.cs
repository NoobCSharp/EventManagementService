using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public class EventStore : IEventStore
    {
        public List<Event> Events { get; set; } = [];

        public void Update(Event @event)
        {
            // В текущей реализации метод Update не выполняет никаких действий, так как коллекция Bookings является списком в памяти.
        }
    }
}
