namespace Domain.Exceptions
{
    public sealed class ActiveBookingLimitExceededException : DomainException
    {
        public ActiveBookingLimitExceededException(string message)
            : base(message, "Conflict")
        {
        }
    }
}
