using EventManagement.Events.Infrastructure.Dtos;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Events.Api.Controllers
{
    /// <summary>
    /// Контроллер событий.  
    /// </summary>
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает объект EventDtoPaginatedResponse.
        /// </summary>
        /// <returns>Объект EventDtoPaginatedResponse сформированный после фильтрации и пагинации.</returns>
        /// <response code="200">События успешно получены</response>
        /// <remarks>
        /// Доступ: открыт для всех пользователей <b>AllowAnonymous</b>.
        ///</remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventDtoPaginatedResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<EventDtoPaginatedResponse>> GetAllEvents([FromQuery] EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            var eventDtoPaginatedResponse = await _eventService.GetEventsAsync(eventFilter, cancellationToken);
            
            return Ok(eventDtoPaginatedResponse);
        }

        /// <summary>
        /// Метод возвращает объект события по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект EventDtoResponse с информацией о событии.</returns>
        /// <response code="200">Событие успешно найдено</response>
        /// <response code="404">Событие не найдено</response>
        /// <remarks>
        /// Доступ: открыт для всех пользователей <b>AllowAnonymous</b>.
        ///</remarks>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventDtoResponse>> GetEventById(Guid id, CancellationToken cancellationToken = default)
        {
            var eventDtoResponse = await _eventService.GetEventByIdAsync(id, cancellationToken);
            
            return Ok(eventDtoResponse);
        }

        /// <summary>
        /// Метод возвращает 10 самых популярных событий.
        /// </summary>
        /// <returns>Коллекция объектов <see cref="EventDtoResponse"/> с наибольшим процентом проданных мест.</returns>
        /// <response code="200">Список популярных событий успешно получен</response>
        /// <remarks>
        /// Доступ: открыт для всех пользователей <b>AllowAnonymous</b>.
        /// </remarks>
        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyCollection<EventDtoResponse>>> GetTopEvents(CancellationToken cancellationToken = default)
        {
            var topTenEvents = await _eventService.GetTopEventsAsync(cancellationToken);

            return Ok(topTenEvents);
        }

        /// <summary>
        /// Метод добавляет объект события в коллекцию.
        /// </summary>
        /// <param name="eventDtoRequest">Новый объект события.</param>
        /// <returns>Объект EventDtoResponse с информацией о событии
        /// и заголовок Location, указывающий на метод получения события по Id.</returns>
        /// <response code="201">Событие успешно создано</response>
        /// <remarks>
        /// Доступ: только пользователи с ролью <b>Admin</b>.
        ///</remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EventDtoResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> AddEvent([FromBody] EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {          
            var eventDtoResponse = await _eventService.AddEventAsync(eventDtoRequest, cancellationToken);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = eventDtoResponse.EventId },
                eventDtoResponse);
        }

        /// <summary>
        /// Метод обновляет существующий объект события.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <param name="eventDtoRequest">Объект EventDtoRequest с новыми данными для обновления события.</param>
        /// <response code="204">Событие успешно обновлено</response>
        /// <response code="400">Некорректные данные для обновления события</response>
        /// <response code="404">Событие не найдено</response>
        /// <remarks>
        /// Доступ: только пользователи с ролью <b>Admin</b>.
        ///</remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateEvent(Guid id, [FromBody] EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {
            await _eventService.UpdateEventAsync(id, eventDtoRequest, cancellationToken);
            
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет объект событие по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <response code="204">Событие успешно удалено</response>
        /// <response code="404">Событие не найдено</response>
        /// <remarks>
        /// Доступ: только пользователи с ролью <b>Admin</b>.
        ///</remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken = default)
        {
            await _eventService.RemoveEventAsync(id, cancellationToken);
            
            return NoContent();
        }
    }
}