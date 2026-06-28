using Domain.Enums;

namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, Role role);
    }
}
