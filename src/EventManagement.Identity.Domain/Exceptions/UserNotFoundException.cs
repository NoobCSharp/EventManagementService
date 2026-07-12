using EventManagement.Shared.Exceptions;

namespace EventManagement.Identity.Domain.Exceptions
{
    public class UserNotFoundException : DomainException
    {
        public UserNotFoundException(string message) 
            : base(message, "Not found") { }
    }
}
