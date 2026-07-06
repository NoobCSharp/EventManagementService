using EventManagement.Events.Infrastructure.Dtos;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.Mappers;
using EventManagement.Events.Domain.Entities;
using EventManagement.Events.Domain.Exceptions;

namespace EventManagement.Events.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _eventRepository.GetEventsAsync(eventFilter, cancellationToken);

            return new EventDtoPaginatedResponse
            {
                CurrentPage = pagedResult.Page,
                NumberOnCurrentPage = pagedResult.PageSize,
                TotalEventsCount = pagedResult.TotalCount,
                ResponseEventDtos = pagedResult.Items
                    .Select(EventMapper.EventToResponse)
                    .ToList(),
            };
        }

        public async Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new EventNotFoundException("Событие по указанному Id не найдено!");

            return EventMapper.EventToResponse(existingEvent);
        }

        public async Task<EventDtoResponse> AddEventAsync(EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventDtoRequest.Title))
                throw new EventValidationException("Название события обязательно к заполнению!");

            if (eventDtoRequest.EndAt <= eventDtoRequest.StartAt)
                throw new EventValidationException("Дата окончания события не может быть раньше даты начала события!");

            if (eventDtoRequest.TotalSeats <= 0)
                throw new EventValidationException("Общее количество мест должно быть положительным числом!");

            Event @event = new()
            {
                EventId = Guid.NewGuid(),
                Title = eventDtoRequest.Title,
                Description = eventDtoRequest.Description,
                StartAt = eventDtoRequest.StartAt,
                EndAt = eventDtoRequest.EndAt,
                TotalSeats = eventDtoRequest.TotalSeats,
                AvailableSeats = eventDtoRequest.TotalSeats
            };

            await _eventRepository.AddEventAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            return EventMapper.EventToResponse(@event);
        }

        public async Task UpdateEventAsync(Guid id, EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {
            if (eventDtoRequest.EndAt <= eventDtoRequest.StartAt)
                throw new EventValidationException("Дата окончания события не может быть раньше даты начала события!");

            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new EventNotFoundException("Событие по указанному Id не найдено!");

            // Забронированные места
            var bookedSeats = existingEvent.TotalSeats - existingEvent.AvailableSeats;

            // TODO подумать над тем, что будет,
            // если при обновлении события общее количество мест будет меньше,
            // чем количество уже забронированных мест.
            if (eventDtoRequest.TotalSeats < bookedSeats)
                throw new EventValidationException("Общее количество мест не может быть меньше количества уже забронированных мест!");

            existingEvent.Title = eventDtoRequest.Title;
            existingEvent.Description = eventDtoRequest.Description;
            existingEvent.StartAt = eventDtoRequest.StartAt;
            existingEvent.EndAt = eventDtoRequest.EndAt;
            existingEvent.TotalSeats = eventDtoRequest.TotalSeats;
            existingEvent.AvailableSeats = eventDtoRequest.TotalSeats - bookedSeats;

            await _eventRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveEventAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new EventNotFoundException("Событие по указанному Id не найдено!");

            _eventRepository.RemoveEvent(existingEvent);

            await _eventRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
