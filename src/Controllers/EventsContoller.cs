using EventManagementService.Dtos;
using EventManagementService.Exceptions;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    /// <summary>
    /// Контроллер событий.  
    /// </summary>
    [ApiController]
    [Route("events")]
    public class EventsContoller : ControllerBase
    {
        private  readonly IEventService _eventService;

        public EventsContoller(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает список объектов событий.
        /// </summary>
        /// <returns>Список объектов событий.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ResponseEventDto>> GetAllEvents()
        {
            IEnumerable<ResponseEventDto> events = _eventService.GetEvents();
            return Ok(events);
        }

        /// <summary>
        /// Метод возвращает объект события по Id из списка.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpGet("{id}")]
        public ActionResult<ResponseEventDto> GetEventById(int id)
        {
            ResponseEventDto responseEventDto = _eventService.GetEventById(id)!;

            if (responseEventDto != null)
                return Ok(responseEventDto);

            throw new NotFoundException("Событие по указанному Id не найдено!");
        }

        /// <summary>
        /// Метод добавляет объект события в коллекцию.
        /// </summary>
        /// <param name="requestEventDto">Новый объект события.</param>
        /// <returns>Возвращает новый объект созданного события
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
        /// <param name="requestEventDto">Событие с новыми данными для обновления.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpPut("{id}")]
        public ActionResult ChangeEvent(int id, [FromBody] RequestEventDto requestEventDto)
        {
            var result = _eventService.ChangeEvent(id, requestEventDto);

            if (!result)
                throw new NotFoundException("Событие по указанному Id не найдено!");
            
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет объект событие по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            var result = _eventService.RemoveEvent(id);

            if (!result)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            return NoContent();
        }
    }
}