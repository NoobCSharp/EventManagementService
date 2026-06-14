using Application.Dtos.UserDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterUserRequest request);

        Task<string> LoginAsync(LoginUserRequest request);
    }
}
