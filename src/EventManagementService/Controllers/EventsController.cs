using EventManagementService.Dtos;
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
        private  readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает объект PaginatedResultDto.
        /// </summary>
        /// <returns>Объект PaginatedResultDto сформированный после фильтрации и пагинации.</returns>
        [HttpGet]
        public ActionResult<PaginatedResultDto> GetAllEvents([FromQuery] EventFilter eventFilter)
        {
            PaginatedResultDto paginatedResult = _eventService.GetEvents(eventFilter);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Метод возвращает объект события по Id из списка.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект ResponseEventDto.</returns>
        [HttpGet("{id}")]
        public ActionResult<ResponseEventDto> GetEventById(int id)
        {
            ResponseEventDto responseEventDto = _eventService.GetEventById(id)!;
            return Ok(responseEventDto);
        }

        /// <summary>
        /// Метод добавляет объект события в коллекцию.
        /// </summary>
        /// <param name="requestEventDto">Новый объект события.</param>
        /// <returns>Возвращает новый объект ResponseEventDto созданного события
        /// и заголовок Location, указывающий на метод получения события по Id.</returns>
        [HttpPost]
        public ActionResult<ResponseEventDto> AddEvent([FromBody] RequestEventDto requestEventDto)
        {          
            ResponseEventDto ResponseEventDto = _eventService.AddEvent(requestEventDto);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = ResponseEventDto.Id },
                ResponseEventDto);
        }

        /// <summary>
        /// Метод обновляет существующий объект события.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <param name="requestEventDto">Объект RequestEventDto с новыми данными для обновления.</param>
        [HttpPut("{id}")]
        public ActionResult ChangeEvent(int id, [FromBody] RequestEventDto requestEventDto)
        {
            _eventService.ChangeEvent(id, requestEventDto);
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет объект событие по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            _eventService.RemoveEvent(id);
            return NoContent();
        }
    }
}