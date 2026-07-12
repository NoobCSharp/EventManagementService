using EventManagement.Shared.Exceptions;

namespace EventManagement.Bookings.Domain.Exceptions
{
    public sealed class BookingValidationException : DomainException
    {
        public BookingValidationException(string message)
            : base(message, "Bad request") { }
    }
}
