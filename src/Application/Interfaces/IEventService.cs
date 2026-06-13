using Application.Dtos.EventDtos;
using Application.Filters;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<EventDtoResponse> AddEventAsync(EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default);
        Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default);
        Task RemoveEventAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateEventAsync(Guid id, EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default);
    }
}