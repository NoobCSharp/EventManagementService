using EventManagement.Identity.Application.Dtos;
using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Application.Services;
using EventManagement.Identity.Domain.Entities;
using EventManagement.Identity.Domain.Enums;
using EventManagement.Identity.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagementService.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();

        private UserService CreateUserService()
        {
            return new UserService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenGeneratorMock.Object
            );
        }

        #region LoginUserAsync

        [Fact]
        public async Task LoginUserAsync_ValidCredentials_ShouldReturnToken()
        {
            // Arrange
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

            // Act
            var result = await service.LoginUserAsync(request);

            // Assert
            result.Should().Be("fake-jwt-token");
        }

        [Fact]
        public async Task LoginUserAsync_IfUserNotFound_ShouldThrow_UserNotFoundException()
        {
            // Arrange
            var request = new LoginUserRequest
            {
                Login = "test",
                Password = "123"
            };

            _userRepositoryMock
                .Setup(x => x.GetUserByLoginAsync(request.Login))
                .ReturnsAsync((User?)null);

            var service = CreateUserService();

            // Act & Assert
            await service.Invoking(s => s.LoginUserAsync(request))
                .Should()
                .ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ShouldThrow_UserNotFoundException()
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

            // Act & Assert
            await service.Invoking(s => s.LoginUserAsync(request))
                .Should()
                .ThrowAsync<UserNotFoundException>();
        }

        #endregion

        #region RegisterUserAsync

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
                r => r.AddUserAsync(It.Is<User>(u =>
                    u.Login == request.Login &&
                    u.PasswordHash == "hashed123" &&
                    u.Role == request.Role)),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_UserAlreadyExists_ShouldThrow_UserAlreadyExistsException()
        {
            // Arrange
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

            // Act & Assert
            await service.Invoking(s => s.RegisterUserAsync(request))
                .Should()
                .ThrowAsync<UserAlreadyExistsException>();
        }

        #endregion
    }
}