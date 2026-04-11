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
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt,
            };
        }
    }
}
