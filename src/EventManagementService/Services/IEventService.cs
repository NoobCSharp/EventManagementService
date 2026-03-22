using EventManagementService.Dtos;
using EventManagementService.Filters;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Получает коллекцию отфильтрованных событий.
        /// </summary>
        /// <param name="eventFilter">Фильтр событий</param>
        /// <returns>
        /// Коллекция отфильтрованных событий.
        /// Если события отсутствуют, возвращается пустая коллекция.
        /// </returns>

        PaginatedResultDto GetEvents(EventFilter eventFilter);

        /// <summary>
        /// Получает событие по Id.
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        /// <returns>
        /// Объект события с указанным Id.
        /// </returns>
        ResponseEventDto? GetEventById(int id);

        /// <summary>
        /// Добавляет событие в коллекцию событий.
        /// </summary>
        /// <param name="requestEventDto">
        /// Объект события содержащий необходимую информацию.
        /// </param>
        /// <returns>
        /// Объект нового события.
        /// </returns>
        ResponseEventDto AddEvent(RequestEventDto requestEventDto);

        /// <summary>
        /// Вносит изменение в существующее событие.
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события для внесения изменений.
        /// </param>
        /// <param name="requestEventDto">
        /// Объект события с новыми данными.
        /// </param>
        void ChangeEvent(int id, RequestEventDto requestEventDto);

        /// <summary>
        /// Удаляет событие по Id из коллекции событий.
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        void RemoveEvent(int id);
    }
}
