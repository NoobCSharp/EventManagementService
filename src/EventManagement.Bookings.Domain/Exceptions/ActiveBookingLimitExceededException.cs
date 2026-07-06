using EventManagement.Shared.Exceptions;

namespace EventManagement.Bookings.Domain.Exceptions
{
    public sealed class ActiveBookingLimitExceededException : DomainException
    {
        public ActiveBookingLimitExceededException(string message)
            : base(message, "Conflict")
        {
        }
    }
}
