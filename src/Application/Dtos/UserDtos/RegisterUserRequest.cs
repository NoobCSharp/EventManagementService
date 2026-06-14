using Domain.Enums;

namespace Application.Dtos.UserDtos
{
    public record RegisterUserRequest
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public required string Login { get; init; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Роль пользователя, определяющая его права доступа.
        /// </summary>
        public required Role Role { get; init; }
    }
}
