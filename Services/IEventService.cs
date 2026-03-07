using EventManagementService.Dtos;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Получает список событий.
        /// </summary>
        /// <returns>
        /// Список событий.
        /// Если события отсутствуют, возвращается пустой список.
        /// </returns>
        IEnumerable<Event> GetEvents();

        /// <summary>
        /// Получает событие по Id.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        /// <returns>
        /// Событие с указанным Id.
        /// Если событие не найдено, возвращается null.
        /// </returns>
        Event GetEventById(int id);

        /// <summary>
        /// Добавляет событие в список событий.
        /// </summary>
        /// <param name="eventDto">
        /// Объект события содержащий необходимую информацию.
        /// </param>
        void AddEvent(EventDto eventDto);

        /// <summary>
        /// Вносит изменение в существующее событие.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события для внесения изменений.
        /// </param>
        /// <param name="eventDto">
        /// Объект события с новыми данными.
        /// </param>
        /// <returns>
        /// Результат выполнения операции true/false.
        /// </returns>
        bool ChangeEvent(int id, EventDto eventDto);

        /// <summary>
        /// Удаляет событие из списка событий.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        /// <returns>
        /// Результат выполнения операции true/false.
        /// </returns>
        bool RemoveEvent(int id);
    }
}
