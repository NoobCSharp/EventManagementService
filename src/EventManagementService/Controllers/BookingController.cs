using EventManagementService.Dtos.BookingDtos;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    [ApiController]
    [Route("events")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Метод добавляет объект брони в коллекцию.
        /// </summary>
        /// <param name="id">Id события для добавления брони.</param>
        /// <returns>Возвращает новый объект BookingDtoResponse созданной брони
        /// и заголовок Location, указывающий на метод получения брони по Id.</returns>
        [HttpPost("{id}/book")]
        public async Task<IActionResult> CreateBooking(Guid id)
        {
            var bookingDtoResponse = await _bookingService.CreateBookingAsync(id);

            return AcceptedAtAction(
                nameof(GetBookingById),
                new { id = bookingDtoResponse.BookingId },
                bookingDtoResponse);
        }

        /// <summary>
        /// Метод возвращает объект брони по Id из коллекции.
        /// </summary>
        /// <param name="id">Уникальный идентификатор брони.</param>
        /// <returns>Объект BookingDtoResponse.</returns>
        [HttpGet("/bookings/{id}")]
        public async Task<ActionResult> GetBookingById(Guid id)
        {
            var bookingDtoResponse = await _bookingService.GetBookingByIdAsync(id);
            return Ok(bookingDtoResponse);
        }
    }
}
