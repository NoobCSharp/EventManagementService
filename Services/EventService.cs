using EventManagementService.Dtos;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private static readonly List<Event> _events = new List<Event>();

        public IEnumerable<Event> GetEvents()
        {
            return _events;
        }

        public Event GetEventById(int id)
        {
            return _events.FirstOrDefault(e => e.Id == id)!;
        }

        public void AddEvent(EventDto eventDto)
        {
            Event @event = new()
            {
                Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1,
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartAt = eventDto.StartAt,
                EndAt = eventDto.EndAt
            };

            _events.Add(@event);
        }

        public bool ChangeEvent(int id, EventDto eventDto)
        {
            Event existingEvent = GetEventById(id);

            if (existingEvent != null)
            {
                existingEvent.Title = eventDto.Title;
                existingEvent.Description = eventDto.Description;
                existingEvent.StartAt = eventDto.StartAt;
                existingEvent.EndAt = eventDto.EndAt;

                return true;
            }

            return false;
        }

        public bool RemoveEvent(int id)
        { 
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                _events.Remove(existingEvent);
                return true;
            }

            return false;
        }
    }
}
