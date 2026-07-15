using EventManagement.Identity.Domain.Entities;
using EventManagement.Identity.Domain.Enums;
using EventManagement.Identity.Infrastructure.Repositories;
using EventManagementService.Identity.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.IntegrationTests
{
    public class UserRepositoryTest : IClassFixture<UsersDbFixture>
    {
        private readonly UsersDbFixture _fixture;

        public UserRepositoryTest(UsersDbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUserToDatabase_And_ReturnUser_WithCorrectData()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new UserRepository(context);

            var user = new User()
            {
                UserId = Guid.NewGuid(),
                Login = "TestUser",
                PasswordHash = "hashed_password",
                Role = Role.User
            };

            // Act
            await repository.AddUserAsync(user);
            await context.SaveChangesAsync();

            // Assert
            // Для проверки создаётся отдельный контекст
            // это исключает чтение из кэша и гарантирует, что данные реально записались в базу.
            await using var verifyContext = _fixture.CreateContext();

            var verifyRepository = new UserRepository(verifyContext);
            var retrievedUser = await verifyRepository.GetUserByLoginAsync(user.Login);

            retrievedUser.Should().NotBeNull();
            retrievedUser.UserId.Should().Be(user.UserId);
            retrievedUser.Login.Should().Be(user.Login);
            retrievedUser.PasswordHash.Should().Be(user.PasswordHash);
            retrievedUser.Role.Should().Be(user.Role);
        }

        [Fact]
        public async Task AddUserAsync_ShouldThrow_DbUpdateException_WhenLoginAlreadyExists()
        {
            // Arrange
            await _fixture.ResetAsync();

            await using var context = _fixture.CreateContext();

            var repository = new UserRepository(context);

            var existingUser = new User()
            {
                UserId = Guid.NewGuid(),
                Login = "TestUser",
                PasswordHash = "hashed_password",
                Role = Role.User
            };

            var newUser = new User()
            {
                UserId = Guid.NewGuid(),
                Login = "TestUser",
                PasswordHash = "hashed_password",
                Role = Role.User
            };

            // Act
            await repository.AddUserAsync(existingUser);
            await repository.AddUserAsync(newUser);

            // Assert
            await context
                .Invoking(c => c.SaveChangesAsync())
                .Should()
                .ThrowAsync<DbUpdateException>();
        }
    }
}
