using EventManagement.Identity.Domain.Enums;

namespace EventManagement.Identity.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, Role role);
    }
}
