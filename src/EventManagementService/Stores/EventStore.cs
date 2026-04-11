using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public class EventStore : IEventStore
    {
        public List<Event> Events { get; set; } = [];
    }
}
