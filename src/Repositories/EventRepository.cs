using EventManagementService.Models;

namespace EventManagementService.Repositories
{
    public class EventRepository : IEventRepository
    {
        /// <summary>
        /// Хранилище событий
        /// </summary>
        public List<Event> Events { get; } = 
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
    }

    public interface IEventRepository
    {
        public List<Event> Events { get; }
    }
}
