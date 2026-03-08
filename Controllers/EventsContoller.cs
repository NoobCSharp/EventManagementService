using EventManagementService.Dtos;
using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    /// <summary>
    /// Контроллер событий.  
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsContoller : ControllerBase
    {
        private  readonly IEventService _eventService;

        public EventsContoller(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Метод возвращает список событий.
        /// </summary>
        /// <returns>Список событий.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Event>> GetAllEvents()
        {
            IEnumerable<Event> events = _eventService.GetEvents();
            return Ok(events);
        }

        /// <summary>
        /// Метод возвращает событие по Id из списка.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpGet("{id}")]
        public ActionResult<Event> GetEventById(int id)
        {
            Event existingEvent = _eventService.GetEventById(id);

            if (existingEvent != null)
                return Ok(existingEvent);

            return NotFound("Событие по указанному Id не найдено!");
        }

        /// <summary>
        /// Метод добавляет событие в список.
        /// </summary>
        /// <param name="eventDto">Новое событие.</param>
        [HttpPost]
        public ActionResult AddEvent([FromBody] EventDto eventDto)
        {
            _eventService.AddEvent(eventDto);

            return Created();
        }

        /// <summary>
        /// Метод обновляет существующее событие.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <param name="eventDto">Событие с новыми данными для обновления.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpPut("{id}")]
        public ActionResult ChangeEvent(int id, [FromBody] EventDto eventDto)
        {
            var result = _eventService.ChangeEvent(id, eventDto);

            if (!result) 
                return NotFound("Событие по указанному Id не найдено!");
            
            return NoContent();
        }

        /// <summary>
        /// Метод удаляет событие по Id.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>В случае ошибки возвращаем результат со статусом Not Found.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            var result = _eventService.RemoveEvent(id);

            if (!result) 
                return NotFound("Событие по указанному Id не найдено!");

            return NoContent();
        }
    }
}