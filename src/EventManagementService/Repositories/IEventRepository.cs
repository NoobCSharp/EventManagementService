using EventManagementService.Entities;
using EventManagementService.Filters;
using EventManagementService.Models;

namespace EventManagementService.Repositories
{
    public interface IEventRepository
    {
        /// <summary>
        /// Возвращает постраничный результат событий, удовлетворяющих заданному фильтру.
        /// </summary>
        /// <param name="filter">
        /// Критерии фильтрации и параметры пагинации.
        /// </param>
        /// <returns>
        /// Объект PagedResult включающий список событий после фильтрации и пагинации
        /// </returns>
        Task<PagedResult<Event>> GetEventsAsync(EventFilter filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает событие по Id из хранилища.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события для поиска.
        /// </param>
        /// <returns>
        /// Объект события с указанным Id.
        /// </returns>
        Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет событие в коллекцию событий.
        /// </summary>
        /// <param name="@event">
        /// Объект события содержащий необходимую информацию.
        /// </param>
        Task AddEventAsync(Event @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет событие из хранилища.
        /// </summary>
        /// <param name="@event">
        /// Объект события для удаления.
        /// </param>
        void RemoveEvent(Event @event);
    }
}
