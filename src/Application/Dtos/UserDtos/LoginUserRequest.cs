namespace Application.Dtos.UserDtos
{
    public record LoginUserRequest
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public required string Login { get; init; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public required string Password { get; init; }
    }
}
