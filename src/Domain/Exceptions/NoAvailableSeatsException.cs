namespace Domain.Exceptions
{
    public sealed class NoAvailableSeatsException : DomainException
    {
        public NoAvailableSeatsException(string message)
            : base(message, "No available seats for this event") { }
    }
}