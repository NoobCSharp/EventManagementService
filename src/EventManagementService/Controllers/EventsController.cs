using EventManagementService.Dtos.EventDtos;
using EventManagementService.Filters;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    /// <summary>
    /// Контроллер событий.  
    /// </summary>
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        
        public EventsController(EventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает объект EventDtoPaginatedResponse.
        /// </summary>
        /// <returns>Объект EventDtoPaginatedResponse сформированный после фильтрации и пагинации.</returns>
        /// <response code="200">События успешно получены</response>
        [HttpGet]
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
        /// <returns>Объект EventDtoResponse.</returns>
        /// <response code="200">Событие успешно найдено</response>
        /// <response code="404">Событие не найдено</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventDtoResponse>> GetEventById(Guid id, CancellationToken cancellationToken = default)
        {
            var eventDtoResponse = await _eventService.GetEventByIdAsync(id, cancellationToken);
            
            return Ok(eventDtoResponse);
        }

        /// <summary>
        /// Метод добавляет объект события в коллекцию.
        /// </summary>
        /// <param name="requestEventDto">Новый объект события.</param>
        /// <returns>Возвращает новый объект EventDtoResponse созданного события
        /// и заголовок Location, указывающий на метод получения события по Id.</returns>
        /// <response code="201">Событие успешно создано</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDtoResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> AddEvent([FromBody] EventDtoRequest requestEventDto, CancellationToken cancellationToken = default)
        {          
            var eventDtoResponse = await _eventService.AddEventAsync(requestEventDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = eventDtoResponse.EventId },
                eventDtoResponse);
        }

        /// <summary>
        /// Метод обновляет существующий объект события.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <param name="eventDtoRequest">Объект EventDtoRequest с новыми данными для обновления.</param>
        /// <response code="204">Событие успешно обновлено</response>
        /// <response code="400">Некорректные данные для обновления события</response>
        /// <response code="404">Событие не найдено</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangeEvent(Guid id, [FromBody] EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
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
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken = default)
        {
            await _eventService.RemoveEventAsync(id, cancellationToken);
            
            return NoContent();
        }
    }
}