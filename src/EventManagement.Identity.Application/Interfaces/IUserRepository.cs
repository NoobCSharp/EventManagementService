using EventManagement.Identity.Domain.Entities;

namespace EventManagement.Identity.Application.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Добавляет нового пользователя в хранилище.
        /// </summary>
        /// <param name="user">Объект пользователя для добавление в хранилище</param>
        Task AddUserAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить пользователя по логину из хранилища. Если пользователь не найден, возвращает null.
        /// </summary>
        /// <param name="login">
        /// Логин пользователя для поиска 
        /// </param>
        /// <returns>
        /// Объект пользователя из хранилища
        /// </returns>
        Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохраняет изменения в хранилище данных.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
