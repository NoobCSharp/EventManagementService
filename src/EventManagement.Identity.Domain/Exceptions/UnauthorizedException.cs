using EventManagement.Shared.Exceptions;

namespace EventManagement.Identity.Domain.Exceptions
{
    public sealed class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message) : base(message, "Unauthorized")
        {
        }
    }
}
