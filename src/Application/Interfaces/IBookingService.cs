using Application.Dtos.BookingDtos;

namespace Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDtoResponse> CreateBookingAsync(Guid id, CancellationToken cancellationToken = default);

        Task<BookingDtoResponse> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}