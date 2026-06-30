using EventManagement.Identity.Application.Dtos;

namespace EventManagement.Identity.Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserRequest request);

        Task<string> LoginUserAsync(LoginUserRequest request);
    }
}