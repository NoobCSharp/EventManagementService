namespace EventManagementService.Exceptions
{
    public sealed class NoAvailableSeatsException : DomainException
    {
        public NoAvailableSeatsException(string message)
            : base(message, StatusCodes.Status409Conflict, "No available seats for this event") { }
    }
}