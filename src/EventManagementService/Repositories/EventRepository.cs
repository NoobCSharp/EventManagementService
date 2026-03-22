using EventManagementService.Models;

namespace EventManagementService.Repositories
{
    public class EventRepository : IEventRepository
    {
        public List<Event> Events { get; set; } = [];

        public int GetAvailableId() => 
            Events.Any() ? Events.Max(e => e.Id) + 1 : 1;
    }
}
