using EventManagementService.Dtos;
using EventManagementService.Exceptions;
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

        public PaginatedResultDto GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var filtered = _eventRepository.Events.AsQueryable();

            //TODO Лучше вынести в отдельный класс фильтра EventFilter
            //Коллекция событий после фильтрации
            filtered = filtered.Where(e =>
                (string.IsNullOrWhiteSpace(title) || e.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
                (!from.HasValue || e.StartAt >= from) &&
                (!to.HasValue || e.EndAt <= to)
            );

            //Количество элементов после фильтрации
            int eventCount = filtered.Count();

            //Коллекция событий после пагинации
            filtered = filtered.Skip((page - 1) * pageSize)
                .Take(pageSize);

            //Количество элементов после фильтрации
            int filteredCount = filtered.Count();

            var responseEventDtos = filtered.Select(EventMapper.EventToResponse);

            return new PaginatedResultDto()
            {
                TotalEventsCount = eventCount,
                ResponseEventDtos = responseEventDtos,
                NumberEventsOnCurrentPage = filtered.Count(),
                CurrentPage = page,
            };
        }

        public ResponseEventDto? GetEventById(int id)
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
            Event @event = new()
            {
                Id = _eventRepository.Events.Any() ? _eventRepository.Events.Max(e => e.Id) + 1 : 1,
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
