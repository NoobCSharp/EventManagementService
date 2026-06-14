using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user, CancellationToken cancellationToken = default);

        Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);
    }
}
