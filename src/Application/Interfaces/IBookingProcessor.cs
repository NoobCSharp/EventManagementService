namespace Application.Interfaces
{
    public interface IBookingProcessor
    {
        Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);

        Task ProcessPendingBookingsAsync(CancellationToken cancellationToken = default);
    }
}
