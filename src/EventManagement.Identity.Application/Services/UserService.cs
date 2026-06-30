using EventManagement.Identity.Application.Dtos;
using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Domain.Entities;
using EventManagement.Identity.Domain.Exceptions;

namespace EventManagement.Identity.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> LoginUserAsync(LoginUserRequest request)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login);

            if (existingUser == null)
                throw new UnauthorizedException("Неверный логин или пароль!");
            
            var isValid = _passwordHasher.Verify(request.Password, existingUser.PasswordHash);

            if (!isValid)
                throw new UnauthorizedException("Неверный логин или пароль!");

            return _jwtTokenGenerator.GenerateToken(existingUser.UserId, existingUser.Login, existingUser.Role);
        }

        public async Task RegisterUserAsync(RegisterUserRequest request)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login);

            if (existingUser != null)
                throw new UserAlreadyExistsException("Пользователь с указанным логином уже существует!");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role,
            };

            await _userRepository.AddUserAsync(user);

            await _userRepository.SaveChangesAsync();
        }
    }
}
