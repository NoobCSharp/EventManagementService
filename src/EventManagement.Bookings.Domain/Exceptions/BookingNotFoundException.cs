using EventManagement.Shared.Exceptions;

namespace EventManagement.Bookings.Domain.Exceptions
{
    public sealed class BookingNotFoundException : DomainException
    {
        public BookingNotFoundException(string message)
            : base(message, "Not found") { }
    }
}
