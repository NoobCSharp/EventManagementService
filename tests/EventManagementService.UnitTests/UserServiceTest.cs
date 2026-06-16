using Application.Dtos.UserDtos;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagementService.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private UserService CreateUserService()
        {
            return new UserService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenGeneratorMock.Object,
                _unitOfWorkMock.Object
            );
        }

        #region Successful scenarios for LoginUserAsync

        [Fact]
        public async Task LoginUserAsync_ValidCredentials_ShouldReturnToken()
        {
            // Arrange (подготовка)
            var request = new LoginUserRequest
            {
                Login = "test",
                Password = "123"
            };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = "123",
                Role = Role.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.PasswordHash))
                .Returns(true);

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(user.UserId, user.Login, user.Role))
                .Returns("fake-jwt-token");

            var service = CreateUserService();

            // Act (действие)
            var result = await service.LoginUserAsync(request);

            // Assert (проверка)
            result.Should().Be("fake-jwt-token");
        }

        #endregion

        #region Unsuccessful scenarios for LoginUserAsync 

        [Fact]
        public async Task LoginUserAsync_UserNotFound_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var request = new LoginUserRequest
            {
                Login = "test",
                Password = "123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync((User?)null);

            var service = CreateUserService();

            // Assert (проверка)
            await service.Invoking(s => s.LoginUserAsync(request))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ShouldThrow_BadRequestException()
        {
            // Arrange
            var request = new LoginUserRequest
            {
                Login = "test",
                Password = "wrong"
            };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = "123",
                Role = Role.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.PasswordHash))
                .Returns(false);

            var service = CreateUserService();

            // Assert (проверка)
            await service.Invoking(s => s.LoginUserAsync(request))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        #endregion

        #region Successful scenarios for RegisterUserAsync

        [Fact]
        public async Task RegisterUserAsync_ValidRequest_ShouldCreateUserAndSave()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Login = "new_user",
                Password = "123",
                Role = Role.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(request.Password))
                .Returns("hashed123");

            var service = CreateUserService();

            // Act
            await service.RegisterUserAsync(request);

            // Assert
            _userRepositoryMock.Verify(
                r => r.AddUserAsync(
                    It.Is<User>(
                        u => u.Login == request.Login 
                        && u.PasswordHash == "hashed123" 
                        && u.Role == request.Role)), 
                Times.Once);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(), 
                Times.Once);
        }

        #endregion

        #region Unsuccessful scenarios for RegisterUserAsync 

        [Fact]
        public async Task RegisterUserAsync_UserAlreadyExists_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            var request = new RegisterUserRequest
            {
                Login = "test",
                Password = "123",
                Role = Role.User
            };

            var existingUser = new User
            {
                UserId = Guid.NewGuid(),
                Login = "test",
                PasswordHash = "123",
                Role = Role.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync(existingUser);

            var service = CreateUserService();

            // Assert (проверка)
            await service.Invoking(s => s.RegisterUserAsync(request))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        #endregion
    }
}
