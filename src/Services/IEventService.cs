using EventManagementService.Dtos;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Получает коллекцию событий.
        /// </summary>
        /// <returns>
        /// Коллекция объектов событий.
        /// Если события отсутствуют, возвращается пустая коллекция.
        /// </returns>
        IEnumerable<ResponseEventDto> GetEvents();

        /// <summary>
        /// Получает событие по Id.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        /// <returns>
        /// Объект события с указанным Id.
        /// Если событие не найдено, возвращается null.
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
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события для внесения изменений.
        /// </param>
        /// <param name="requestEventDto">
        /// Объект события с новыми данными.
        /// </param>
        /// <returns>
        /// Результат выполнения операции true/false.
        /// </returns>
        bool ChangeEvent(int id, RequestEventDto requestEventDto);

        /// <summary>
        /// Удаляет событие из коллекции событий.
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
