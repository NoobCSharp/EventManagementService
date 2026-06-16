using Application.Dtos.UserDtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> LoginUserAsync(LoginUserRequest request)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login);

            if (existingUser == null)
                throw new NotFoundException("Пользователь не найден!");
            
            var isValid = _passwordHasher.Verify(request.Password, existingUser.PasswordHash);

            if (!isValid)
                throw new BadRequestException("Неверный логин или пароль!");

            return _jwtTokenGenerator.GenerateToken(existingUser.UserId, existingUser.Login, existingUser.Role);
        }

        public async Task RegisterUserAsync(RegisterUserRequest request)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login);

            if (existingUser != null)
                throw new BadRequestException("Пользователь с указанным логином уже существует!");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role,
            };

            await _userRepository.AddUserAsync(user);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
