using Application.Dtos.BookingDtos;
using Domain.Entities;

namespace Application.Mappers
{
    public static class BookingMapper
    {
        public static BookingDtoResponse BookingToResponse(Booking booking)
        {
            return new BookingDtoResponse
            {
                BookingId = booking.BookingId,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt,
            };
        }
    }
}
