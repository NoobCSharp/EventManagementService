using EventManagementService.Dtos.EventDtos;
using EventManagementService.Filters;
using EventManagementService.Models;

namespace EventManagementService.Repositories
{
    public interface IEventRepository
    {
        /// <summary>
        /// Получает коллекцию отфильтрованных событий.
        /// </summary>
        /// <param name="eventFilter">Фильтр событий</param>
        /// <returns>
        /// Коллекция отфильтрованных событий.
        /// Если события отсутствуют, возвращается пустая коллекция.
        /// </returns>
        Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default);

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
        /// Вносит изменение в существующее событие.
        /// </summary>
        /// <param name="@event">
        /// Объект события содержащий новые данные для внесения изменений.
        /// </param>
        Task UpdateEventAsync(Event @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет событие из хранилища.
        /// </summary>
        /// <param name="@event">
        /// Объект события для удаления.
        /// </param>
        Task RemoveEventAsync(Event @event, CancellationToken cancellationToken = default);
    }
}
