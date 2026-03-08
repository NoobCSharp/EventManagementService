using EventManagementService.Dtos;
using EventManagementService.Mappers;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        /// <summary>
        /// Хранилище событий
        /// </summary>
        private static readonly List<Event> _events = new List<Event>();

        public IEnumerable<ResponseEventDto> GetEvents()
        {    
            return _events.Select(EventMapper.EventToResponse);
        }

        public ResponseEventDto? GetEventById(int id)
        {
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
                return EventMapper.EventToResponse(existingEvent);

            return default;
        }

        public ResponseEventDto AddEvent(RequestEventDto requestEventDto)
        {
            Event @event = new()
            {
                Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1,
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt
            };

            _events.Add(@event);

            return EventMapper.EventToResponse(@event);
        }

        public bool ChangeEvent(int id, RequestEventDto requestEventDto)
        {
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                existingEvent.Title = requestEventDto.Title;
                existingEvent.Description = requestEventDto.Description;
                existingEvent.StartAt = requestEventDto.StartAt;
                existingEvent.EndAt = requestEventDto.EndAt;

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
