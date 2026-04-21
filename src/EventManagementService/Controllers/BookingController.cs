using EventManagementService.Dtos.BookingDtos;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Метод создает бронирование для события.
        /// </summary>
        /// <param name="id">Идентификатор события для добавления брони</param>
        /// <returns>Созданная бронь и заголовок Location, 
        /// указывающий на метод получения брони по Id.</returns>
        /// <response code="202">Бронь успешно создана</response>
        /// <response code="404">Событие не найдено</response>
        /// <response code="409">Нет доступных мест на событие</response>
        [HttpPost("events/{id}/book")]
        [ProducesResponseType(typeof(BookingDtoResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDtoResponse>> CreateBooking(Guid id)
        {
            var bookingDtoResponse = await _bookingService.CreateBookingAsync(id);

            return AcceptedAtAction(
                nameof(GetBookingById),
                new { id = bookingDtoResponse.Id },
                bookingDtoResponse);
        }

        /// <summary>
        /// Метод возвращает объект брони по идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор брони.</param>
        /// <returns>Объект брони</returns>
        /// <response code="200">Бронь успешно найдена</response>
        /// <response code="404">Бронь не найдена</response>
        [HttpGet("bookings/{id}")]
        [ProducesResponseType(typeof(BookingDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDtoResponse>> GetBookingById(Guid id)
        {
            var bookingDtoResponse = await _bookingService.GetBookingByIdAsync(id);

            return Ok(bookingDtoResponse);
        }
    }
}
