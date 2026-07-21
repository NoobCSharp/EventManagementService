namespace EventManagement.Events.Application.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Получает объект указанного типа из кэша по ключу.
        /// </summary>
        /// <param name="key">Ключ записи в кэше.</param>
        /// <returns>
        /// Объект типа <typeparamref name="T"/>, если запись с указанным ключом найдена в кэше;
        /// в противном случае — <see langword="null"/>.
        /// </returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохраняет объект в кэш на указанное время.
        /// </summary>
        /// <typeparam name="T">Тип сохраняемого объекта.</typeparam>
        /// <param name="key">Ключ записи в кэше.</param>
        /// <param name="value">Объект для сохранения.</param>
        /// <param name="expiration">Время жизни записи в кэше TTL.</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет запись из кэша по ключу.
        /// </summary>
        /// <param name="key">Ключ записи в кэше.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
