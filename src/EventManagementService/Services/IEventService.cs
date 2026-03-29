using EventManagementService.Dtos.EventDtos;
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

        EventDtoPaginatedResponse GetEvents(EventFilter eventFilter);

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
        EventDtoResponse GetEventById(Guid id);

        /// <summary>
        /// Добавляет событие в коллекцию событий.
        /// </summary>
        /// <param name="requestEventDto">
        /// Объект события содержащий необходимую информацию.
        /// </param>
        /// <returns>
        /// Объект нового события.
        /// </returns>
        EventDtoResponse AddEvent(EventDtoRequest requestEventDto);

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
        void ChangeEvent(Guid id, EventDtoRequest requestEventDto);

        /// <summary>
        /// Удаляет событие по Id из коллекции событий.
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события.
        /// </param>
        void RemoveEvent(Guid id);
    }
}
