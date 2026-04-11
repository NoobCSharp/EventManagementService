using EventManagementService.Dtos.EventDtos;
using EventManagementService.Models;

namespace EventManagementService.Mappers
{
    public static class EventMapper
    {
        public static EventDtoResponse EventToResponse(Event @event)
        {
            return new EventDtoResponse
            {
                EventId = @event.EventId,
                Title = @event.Title,
                Description = @event.Description,
                StartAt = @event.StartAt,
                EndAt = @event.EndAt
            };
        }
    }
}