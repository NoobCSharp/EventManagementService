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

        public async Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, eventFilter.Page);
            var pageSize = Math.Max(1, eventFilter.PageSize);

            var query = _appDbContext.Events.Where(e =>
                (string.IsNullOrWhiteSpace(eventFilter.Title) || e.Title.Contains(eventFilter.Title)) &&
                (!eventFilter.From.HasValue || e.StartAt >= eventFilter.From) &&
                (!eventFilter.To.HasValue || e.EndAt <= eventFilter.To));

            var total = await query.CountAsync(cancellationToken);

            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToListAsync(cancellationToken);

            var items = entities.Select(EventMapper.EventToResponse).ToList();

            return new EventDtoPaginatedResponse
            {
                TotalEventsCount = total,
                ResponseEventDtos = items,
                NumberEventsOnCurrentPage = items.Count,
                CurrentPage = page
            };
        }

        public async Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Event? existingEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            return EventMapper.EventToResponse(existingEvent);
        }

        public async Task<EventDtoResponse> AddEventAsync(EventDtoRequest requestEventDto, CancellationToken cancellationToken)
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

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return EventMapper.EventToResponse(@event);
        }

        public async Task ChangeEventAsync(Guid id, EventDtoRequest requestEventDto, CancellationToken cancellationToken)
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

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveEventAsync(Guid id, CancellationToken cancellationToken)
        { 
            var existingEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            _appDbContext.Events.Remove(existingEvent);

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}