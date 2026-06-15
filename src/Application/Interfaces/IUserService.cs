using Application.Dtos.UserDtos;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserRequest request);

        Task<string> LoginUserAsync(LoginUserRequest request);
    }
}