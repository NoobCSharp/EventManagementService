using EventManagement.Shared.Exceptions;

namespace EventManagement.Events.Domain.Exceptions
{
    public sealed class EventNotFoundException : DomainException
    {
        public EventNotFoundException(string message) : base(message, "UserAlreadyExists")
        {
        }
    }
}
