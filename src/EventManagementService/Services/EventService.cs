using EventManagementService.DataAccess;
using EventManagementService.Dtos.EventDtos;
using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Mappers;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _appDbContext;

        public EventService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter)
        {
            var events = _appDbContext.Events.AsQueryable();

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

            var eventDtoPaginatedResponse = new EventDtoPaginatedResponse()
            {
                TotalEventsCount = eventCount,
                ResponseEventDtos = responseEventDtos,
                NumberEventsOnCurrentPage = eventsOnPageCount,
                CurrentPage = eventFilter.Page,
            };

            _appDbContext.SaveChangesAsync();

            return Task.FromResult(eventDtoPaginatedResponse);
        }

        public Task<EventDtoResponse> GetEventByIdAsync(Guid id)
        {
            Event existingEvent = _appDbContext.Events.FirstOrDefault(e => e.EventId == id)!;

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            var eventDtoResponse = EventMapper.EventToResponse(existingEvent);

            return Task.FromResult(eventDtoResponse);
        }

        public async Task<EventDtoResponse> AddEventAsync(EventDtoRequest requestEventDto)
        {
            if (string.IsNullOrWhiteSpace(requestEventDto.Title))
                throw new BadRequestException("Название события обязательно к заполнению!");
           
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            if (requestEventDto.TotalSeats <= 0)
                throw new BadRequestException("Общее количество мест должно быть положительным числом!");

            Event @event = new()
            {
                EventId = Guid.NewGuid(),
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt,
                TotalSeats = requestEventDto.TotalSeats,
                AvailableSeats = requestEventDto.TotalSeats
            };

            _appDbContext.Events.Add(@event);

            await _appDbContext.SaveChangesAsync();

            return EventMapper.EventToResponse(@event);
        }

        public async Task ChangeEvent(Guid id, EventDtoRequest requestEventDto)
        {
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            var existingEvent = await _appDbContext.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            existingEvent.Title = requestEventDto.Title;
            existingEvent.Description = requestEventDto.Description;
            existingEvent.StartAt = requestEventDto.StartAt;
            existingEvent.EndAt = requestEventDto.EndAt;

            await _appDbContext.SaveChangesAsync();
        }

        public async Task RemoveEventAsync(Guid id)
        { 
            var existingEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            _appDbContext.Events.Remove(existingEvent);

            await _appDbContext.SaveChangesAsync();
        }
    }
}