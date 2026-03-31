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
        /// Метод возвращает объект PaginatedResultDto.
        /// </summary>
        /// <returns>Объект PaginatedResultDto сформированный после фильтрации и пагинации.</returns>
        [HttpGet]
        public async Task<ActionResult> GetAllEvents([FromQuery] EventFilter eventFilter)
        {
            var paginatedResult = await _eventService.GetEventsAsync(eventFilter);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Метод возвращает объект события по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект ResponseEventDto.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEventById(Guid id)
        {
            var responseEventDto = await _eventService.GetEventByIdAsync(id);
            return Ok(responseEventDto);
        }

        /// <summary>
        /// Метод добавляет объект события в коллекцию.
        /// </summary>
        /// <param name="requestEventDto">Новый объект события.</param>
        /// <returns>Возвращает новый объект ResponseEventDto созданного события
        /// и заголовок Location, указывающий на метод получения события по Id.</returns>
        [HttpPost]
        public async Task<ActionResult> AddEvent([FromBody] EventDtoRequest requestEventDto)
        {          
            var responseEventDto = await _eventService.AddEventAsync(requestEventDto);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = responseEventDto.EventId },
                responseEventDto);
        }

        /// <summary>
        /// Метод обновляет существующий объект события.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <param name="requestEventDto">Объект RequestEventDto с новыми данными для обновления.</param>
        [HttpPut("{id}")]
        public async Task<ActionResult> ChangeEvent(Guid id, [FromBody] EventDtoRequest requestEventDto)
        {
            await _eventService.ChangeEvent(id, requestEventDto);
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет объект событие по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            await _eventService.RemoveEventAsync(id);
            return NoContent();
        }
    }
}