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

        public async Task<string> LoginUserAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (existingUser == null)
                throw new UserNotFoundException("Неверный логин или пароль!");
            
            var isValid = _passwordHasher.Verify(request.Password, existingUser.PasswordHash);

            if (!isValid)
                throw new UserNotFoundException("Неверный логин или пароль!");

            return _jwtTokenGenerator.GenerateToken(existingUser.UserId, existingUser.Login, existingUser.Role);
        }

        public async Task RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (existingUser != null)
                throw new UserAlreadyExistsException("Пользователь с указанным логином уже существует!");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role,
            };

            await _userRepository.AddUserAsync(user, cancellationToken);

            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
