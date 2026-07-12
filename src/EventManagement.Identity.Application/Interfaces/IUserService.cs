using EventManagement.Identity.Application.Dtos;

namespace EventManagement.Identity.Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

        Task<string> LoginUserAsync(LoginUserRequest request, CancellationToken cancellationToken = default);
    }
}