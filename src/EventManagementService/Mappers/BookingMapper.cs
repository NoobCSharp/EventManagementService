using EventManagementService.Dtos.BookingDtos;
using EventManagementService.Models;

namespace EventManagementService.Mappers
{
    public static class BookingMapper
    {
        public static BookingDtoResponse BookingToResponse(Booking booking)
        {
            return new BookingDtoResponse
            {
                BookingId = booking.BookingId,
                EventId = booking.EventId,
                Status = booking.Status
            };
        }
    }
}
