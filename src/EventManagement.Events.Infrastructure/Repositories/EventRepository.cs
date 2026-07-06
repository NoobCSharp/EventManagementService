using EventManagement.Events.Infrastructure.Common;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Domain.Entities;
using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Events.Infrastructure.Repositories
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

        public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == id, cancellationToken);
        }

        public async Task<PagedResult<Event>> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, eventFilter.Page);
            var pageSize = Math.Max(1, eventFilter.PageSize);

            var query = _appDbContext.Events.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(eventFilter.Title))
                query = query.Where(e => e.Title.Contains(eventFilter.Title));

            if (eventFilter.From.HasValue)
                query = query.Where(e => e.StartAt >= eventFilter.From);

            if (eventFilter.To.HasValue)
                query = query.Where(e => e.EndAt <= eventFilter.To);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.StartAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Event>
            {
                Page = page,
                PageSize = items.Count,
                TotalCount = total,
                Items = items
            };
        }

        public void RemoveEvent(Event @event)
        {
            _appDbContext.Events.Remove(@event);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
