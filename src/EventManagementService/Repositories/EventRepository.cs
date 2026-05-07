using EventManagementService.DataAccess;
using EventManagementService.Dtos.EventDtos;
using EventManagementService.Filters;
using EventManagementService.Mappers;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _appDbContext;

        public EventRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddEventAsync(Event @event, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Events.AddAsync(@event, cancellationToken);
        }

        public async Task UpdateEventAsync(Event @event, CancellationToken cancellationToken = default)
        {
            _appDbContext.Update(@event);
        }

        public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == id, cancellationToken);
        }

        public async Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, eventFilter.Page);
            var pageSize = Math.Max(1, eventFilter.PageSize);

            var query = _appDbContext.Events.AsNoTracking().Where(e =>
                (string.IsNullOrWhiteSpace(eventFilter.Title) || e.Title.Contains(eventFilter.Title)) &&
                (!eventFilter.From.HasValue || e.StartAt >= eventFilter.From) &&
                (!eventFilter.To.HasValue || e.EndAt <= eventFilter.To));

            var total = await query.CountAsync(cancellationToken);

            var events = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = events.Select(EventMapper.EventToResponse).ToList();

            return new EventDtoPaginatedResponse
            {
                TotalEventsCount = total,
                ResponseEventDtos = items,
                NumberEventsOnCurrentPage = items.Count,
                CurrentPage = page
            };
        }

        public async Task RemoveEventAsync(Event @event, CancellationToken cancellationToken = default)
        {
            _appDbContext.Events.Remove(@event);
        }
    }
}
