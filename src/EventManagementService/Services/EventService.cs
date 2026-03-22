using EventManagementService.Dtos;
using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Mappers;
using EventManagementService.Models;
using EventManagementService.Repositories;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public PaginatedResultDto GetEvents(EventFilter eventFilter)
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

            return new PaginatedResultDto()
            {
                TotalEventsCount = eventCount,
                ResponseEventDtos = responseEventDtos,
                NumberEventsOnCurrentPage = eventsOnPageCount,
                CurrentPage = eventFilter.Page,
            };
        }

        public ResponseEventDto GetEventById(int id)
        {
            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                return EventMapper.EventToResponse(existingEvent);
            }
            else
            {
                throw new NotFoundException("Событие по указанному Id не найдено!");
            }
        }

        public ResponseEventDto AddEvent(RequestEventDto requestEventDto)
        {
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadHttpRequestException("Дата окончания события не может быть раньше даты начала события!");

            //Подумать почему метод GetAvailableId в тесте возвращает 0 если не настраивать Mock
            int id = _eventRepository.GetAvailableId();

            Event @event = new()
            {
                Id = id,
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt
            };

            _eventRepository.Events.Add(@event);

            return EventMapper.EventToResponse(@event);
        }

        public void ChangeEvent(int id, RequestEventDto requestEventDto)
        {
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadHttpRequestException("Дата окончания события не может быть раньше даты начала события!");

            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                existingEvent.Title = requestEventDto.Title;
                existingEvent.Description = requestEventDto.Description;
                existingEvent.StartAt = requestEventDto.StartAt;
                existingEvent.EndAt = requestEventDto.EndAt;
            }
            else
            {
                throw new NotFoundException("Событие по указанному Id не найдено!");
            }
        }

        public void RemoveEvent(int id)
        { 
            Event existingEvent = _eventRepository.Events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                _eventRepository.Events.Remove(existingEvent);
            }
            else
            {
                throw new NotFoundException("Событие по указанному Id не найдено!");
            }
        }
    }
}
