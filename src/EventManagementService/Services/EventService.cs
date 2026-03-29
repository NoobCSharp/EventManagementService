using EventManagementService.Dtos.EventDtos;
using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Mappers;
using EventManagementService.Models;
using EventManagementService.Stores;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly IEventStore _eventRepository;

        public EventService(IEventStore eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public EventDtoPaginatedResponse GetEvents(EventFilter eventFilter)
        {
            var events = _eventRepository.Events.AsQueryable();

            //TODO Лучше вынести в отдельный класс фильтра EventFilter
            //Коллекция событий после фильтрации
            var filtered = events.Where(e =>
                (string.IsNullOrWhiteSpace(eventFilter.Title) || e.Title.Contains(eventFilter.Title, StringComparison.OrdinalIgnoreCase)) &&
                (!eventFilter.From.HasValue || e.StartAt >= eventFilter.From) &&
                (!eventFilter.To.HasValue || e.EndAt <= eventFilter.To)
            );

            //Количество элементов после фильтрации
            int eventCount = filtered.Count();

            //Коллекция событий после пагинации
            var eventsOnPage = filtered.Skip((eventFilter.Page - 1) * eventFilter.PageSize)
                .Take(eventFilter.PageSize);

            //Количество элементов после пагинации
            int eventsOnPageCount = eventsOnPage.Count();

            var responseEventDtos = eventsOnPage.Select(EventMapper.EventToResponse);

            return new EventDtoPaginatedResponse()
            {
                TotalEventsCount = eventCount,
                ResponseEventDtos = responseEventDtos,
                NumberEventsOnCurrentPage = eventsOnPageCount,
                CurrentPage = eventFilter.Page,
            };
        }

        public EventDtoResponse GetEventById(Guid id)
        {
            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.EvendId == id)!;

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            return EventMapper.EventToResponse(existingEvent);
        }

        public EventDtoResponse AddEvent(EventDtoRequest requestEventDto)
        {
            if (string.IsNullOrWhiteSpace(requestEventDto.Title))
                throw new BadRequestException("Название события обязательно к заполнению!");
           
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            Event @event = new()
            {
                EvendId = Guid.NewGuid(),
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt
            };

            _eventRepository.Events.Add(@event);

            return EventMapper.EventToResponse(@event);
        }

        public void ChangeEvent(Guid id, EventDtoRequest requestEventDto)
        {
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.EvendId == id)!;

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            existingEvent.Title = requestEventDto.Title;
            existingEvent.Description = requestEventDto.Description;
            existingEvent.StartAt = requestEventDto.StartAt;
            existingEvent.EndAt = requestEventDto.EndAt;
        }

        public void RemoveEvent(Guid id)
        { 
            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.EvendId == id)!;

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");
            
            _eventRepository.Events.Remove(existingEvent);
        }
    }
}
