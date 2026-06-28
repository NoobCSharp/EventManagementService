using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public required string Login { get; set; }

        /// <summary>
        /// Пароль пользователя в виде хэша.
        /// </summary>
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Роль пользователя, определяющая его права доступа.
        /// </summary>
        public required Role Role { get; set; }

        public User()
        {
        }
    }
}
