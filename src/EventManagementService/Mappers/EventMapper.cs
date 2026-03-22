using EventManagementService.Dtos;
using EventManagementService.Models;

namespace EventManagementService.Mappers
{
    public static class EventMapper
    {
        public static ResponseEventDto EventToResponse(Event @event)
        {
            return new ResponseEventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartAt = @event.StartAt,
                EndAt = @event.EndAt
            };
        }
    }
}