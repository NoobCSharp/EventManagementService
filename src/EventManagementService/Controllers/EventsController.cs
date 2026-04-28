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
        private readonly IEventService _eventService;
        
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает объект EventDtoPaginatedResponse.
        /// </summary>
        /// <returns>Объект EventDtoPaginatedResponse сформированный после фильтрации и пагинации.</returns>
        [HttpGet]
        public async Task<ActionResult> GetAllEvents([FromQuery] EventFilter eventFilter, CancellationToken cancellationToken)
        {
            var eventDtoPaginatedResponse = await _eventService.GetEventsAsync(eventFilter, cancellationToken);
            return Ok(eventDtoPaginatedResponse);
        }

        /// <summary>
        /// Метод возвращает объект события по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект EventDtoResponse.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEventById(Guid id, CancellationToken cancellationToken)
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
        [HttpPost]
        [ProducesResponseType(typeof(EventDtoResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> AddEvent([FromBody] EventDtoRequest requestEventDto, CancellationToken cancellationToken)
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
        [HttpPut("{id}")]
        public async Task<ActionResult> ChangeEvent(Guid id, [FromBody] EventDtoRequest eventDtoRequest, CancellationToken cancellationToken)
        {
            await _eventService.ChangeEventAsync(id, eventDtoRequest, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет объект событие по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken)
        {
            await _eventService.RemoveEventAsync(id, cancellationToken);
            return NoContent();
        }
    }
}