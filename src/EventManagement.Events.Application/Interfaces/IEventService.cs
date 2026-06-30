using EventManagement.Events.Application.Dtos;
using EventManagement.Events.Application.Filters;

namespace EventManagement.Events.Application.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Добавление нового события. При добавлении события выполняются следующие проверки:
        /// - Название события обязательно к заполнению.
        /// - Дата окончания события не может быть раньше даты начала события.
        /// - Общее количество мест должно быть положительным числом.
        /// Если событие некорректно, будет выброшено исключение BadRequestException.
        /// </summary>
        /// <param name="eventDtoRequest">Данные для создания нового события.</param>
        /// <returns>Созданное событие.</returns>
        Task<EventDtoResponse> AddEventAsync(EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение события по его уникальному идентификатору. 
        /// Если событие с указанным Id не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект события.</returns>
        Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение списка событий с поддержкой фильтрации по названию и диапазону дат, а также с поддержкой пагинации.
        /// </summary>
        /// <param name="eventFilter">Фильтр для поиска событий.</param>
        /// <returns>Список отфильтрованных событий с поддержкой пагинации.</returns>
        Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет событие по его уникальному идентификатору.
        /// Если событие с указанным Id не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Идентификатор события для удаления.</param>
        Task RemoveEventAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновление существующего события. При обновлении события выполняются следующие проверки:
        /// - дата окончания события не может быть раньше даты начала события.
        /// - общее количество мест не может быть меньше количества уже забронированных мест.
        /// Если событие некорректно, будет выброшено исключение BadRequestException.
        /// Если событие не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Идентификатор события для обновления.</param>
        /// <param name="eventDtoRequest">Данные для обновления события.</param>
        Task UpdateEventAsync(Guid id, EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default);
    }
}