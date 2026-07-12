using EventManagement.Bookings.Application.Dtos;
using EventManagement.Bookings.Domain.Entities;

namespace EventManagement.Bookings.Application.Mappers
{
    public static class BookingMapper
    {
        public static BookingDtoResponse BookingToResponse(Booking booking)
        {
            return new BookingDtoResponse
            {
                BookingId = booking.BookingId,
                EventId = booking.EventId,
                UserId = booking.UserId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
            };
        }
    }
}
