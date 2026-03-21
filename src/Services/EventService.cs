using EventManagementService.Dtos;
using EventManagementService.Exceptions;
using EventManagementService.Mappers;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        /// <summary>
        /// Хранилище событий
        /// </summary>
        private static readonly List<Event> _events = 
            [
                new Event() { Id = 1, Title = "LMA", Description = "Description1", StartAt = new DateTime(2026, 03, 10), EndAt = new DateTime(2026, 03, 11) },
                new Event() { Id = 2, Title = "LMB", Description = "Description2", StartAt = new DateTime(2026, 03, 12), EndAt = new DateTime(2026, 03, 13) },
                new Event() { Id = 3, Title = "LMC", Description = "Description3", StartAt = new DateTime(2026, 03, 14), EndAt = new DateTime(2026, 03, 15) },
                new Event() { Id = 4, Title = "LMD", Description = "Description4", StartAt = new DateTime(2026, 03, 16), EndAt = new DateTime(2026, 03, 17) },
                new Event() { Id = 5, Title = "LME", Description = "Description5", StartAt = new DateTime(2026, 03, 18), EndAt = new DateTime(2026, 03, 19) },
                new Event() { Id = 6, Title = "LMF", Description = "Description6", StartAt = new DateTime(2026, 03, 20), EndAt = new DateTime(2026, 03, 21) },
                new Event() { Id = 7, Title = "LMG", Description = "Description7", StartAt = new DateTime(2026, 03, 22), EndAt = new DateTime(2026, 03, 23) },
                new Event() { Id = 8, Title = "LMH", Description = "Description8", StartAt = new DateTime(2026, 03, 24), EndAt = new DateTime(2026, 03, 25) },
                new Event() { Id = 9, Title = "LMJ", Description = "Description9", StartAt = new DateTime(2026, 03, 26), EndAt = new DateTime(2026, 03, 27) }
            ];

        public PaginatedResultDto GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var filtered = _events.AsQueryable();

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
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

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
                Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1,
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt
            };

            _events.Add(@event);

            return EventMapper.EventToResponse(@event);
        }

        public void ChangeEvent(int id, RequestEventDto requestEventDto)
        {
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

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
            Event existingEvent = _events.FirstOrDefault(e => e.Id == id)!;

            if (existingEvent != null)
            {
                _events.Remove(existingEvent);
            }
            else
            {
                throw new NotFoundException("Событие по указанному Id не найдено!");
            }
        }
    }
}
