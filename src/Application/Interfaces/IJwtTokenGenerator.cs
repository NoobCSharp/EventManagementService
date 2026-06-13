namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, string role);
    }
}
