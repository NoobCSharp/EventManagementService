using EventManagement.Events.Application.Dtos;
using EventManagement.Events.Domain.Entities;

namespace EventManagement.Events.Application.Mappers
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
                EndAt = @event.EndAt,
                TotalSeats = @event.TotalSeats,
                AvailableSeats = @event.AvailableSeats
            };
        }
    }
}