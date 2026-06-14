using Application.Dtos.BookingDtos;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDtoResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

        Task<BookingDtoResponse> CreateBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);

        Task RemoveBookingAsync(Guid bookingId, Guid userId, Role role, CancellationToken cancellationToken = default);
    }
}