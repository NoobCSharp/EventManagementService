using EventManagement.Shared.Exceptions;

namespace EventManagement.Identity.Domain.Exceptions
{
    public sealed class UserAlreadyExistsException : DomainException
    {
        public UserAlreadyExistsException(string message) : base(message, "UserAlreadyExists")
        {
        }
    }
}
