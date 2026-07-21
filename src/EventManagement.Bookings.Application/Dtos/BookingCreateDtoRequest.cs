namespace EventManagement.Bookings.Application.Dtos
{
    public record BookingCreateDtoRequest
    {
        public int SeatCount { get; init; }
    }
}
