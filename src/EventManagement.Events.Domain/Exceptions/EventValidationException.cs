using EventManagement.Shared.Exceptions;

namespace EventManagement.Events.Domain.Exceptions
{
    public class EventValidationException : DomainException
    {
        public EventValidationException(string message) 
            : base(message, "Bad request") { }
    }
}
