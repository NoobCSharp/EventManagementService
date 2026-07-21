using EventManagement.Events.Infrastructure.Common;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Domain.Entities;

namespace EventManagement.Events.Infrastructure.Interfaces
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
        /// Возвращает события с наибольшим процентом проданных мест.
        /// </summary>
        /// <param name="count">
        /// Количество событий, которое необходимо вернуть. По умолчанию — 10.
        /// </param>
        /// <returns>
        /// Коллекция событий, отсортированных по убыванию процента проданных мест.
        /// </returns>
        Task<IReadOnlyCollection<Event>> GetTopEventsAsync(int count = 10, CancellationToken cancellationToken = default);

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
        /// Добавляет событие в хранилище.
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

        /// <summary>
        /// Сохраняет изменения в хранилище данных.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
