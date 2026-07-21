using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Dtos;
using EventManagement.Bookings.Api.Extensions;

namespace EventManagement.Bookings.Api.Controllers
{
    /// <summary>
    /// Контроллер обработки броней.  
    /// </summary>
    /// <remarks>
    /// Доступ: открыт для всех зарегистрированных пользователей и администраторов.
    ///</remarks>
    [Authorize]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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
        public async Task<ActionResult<BookingDtoResponse>> GetBookingById(Guid id, CancellationToken cancellationToken = default)
        {
            var bookingDtoResponse = await _bookingService.GetBookingByIdAsync(id, cancellationToken);

            return Ok(bookingDtoResponse);
        }

        /// <summary>
        /// Метод создает бронирование для события.
        /// </summary>
        /// <param name="id">Идентификатор события для добавления брони</param>
        /// <param name="request">Объект с информацией для бронирования</param>
        /// <returns>Созданная бронь и заголовок Location, 
        /// указывающий на метод получения брони по Id.</returns>
        /// <response code="202">Бронь успешно создана</response>
        /// <response code="400">Событие уже началось или окончено</response>
        /// <response code="404">Событие не найдено</response>
        /// <response code="409">Нет доступных мест на событие или превышен лимит бронирований</response>   
        [HttpPost("events/{id}/book")]
        [ProducesResponseType(typeof(BookingDtoResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDtoResponse>> CreateBooking(Guid id, [FromBody] BookingCreateDtoRequest request, CancellationToken cancellationToken = default)
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);

            var bookingDtoResponse = await _bookingService.CreateBookingAsync(id, userId, request, cancellationToken);

            return AcceptedAtAction(
                nameof(GetBookingById),
                new { id = bookingDtoResponse.BookingId },
                bookingDtoResponse);
        }

        /// <summary>
        /// Метод отменяет бронирование по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор события для дальнейшей отмены</param>
        /// <response code="204">Бронь успешно отменена</response>
        /// <response code="400">Событие уже началось или окончено или бронь уже отменена</response>
        /// <response code="403">Не достаточно прав</response>
        /// <response code="404">Бронь не найдена</response>
        [HttpDelete("bookings/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            var userRole = ClaimsPrincipalExtensions.GetUserRole(User);

            await _bookingService.CancelBookingAsync(id, userId, userRole, cancellationToken);

            return NoContent();
        }
    }
}