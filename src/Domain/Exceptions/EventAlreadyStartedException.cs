namespace Domain.Exceptions
{
    public sealed class EventAlreadyStartedException : DomainException
    {
        public EventAlreadyStartedException(string message)
            : base(message, "Bad request") 
        {
        }
    }
}
