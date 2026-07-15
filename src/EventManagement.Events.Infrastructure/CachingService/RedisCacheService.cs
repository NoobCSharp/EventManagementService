using EventManagement.Events.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EventManagement.Events.Infrastructure.CachingService
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger)
        {
            _db = connection.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Получает объект из Redis по ключу.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisValue = await _db.StringGetAsync(key);

                if (redisValue.HasValue)
                {
                    return JsonSerializer.Deserialize<T>(redisValue.ToString());
                }

               return default;
            }
            catch (Exception ex)
            {
                // Redis недоступен - продолжаем работу без кэша
                _logger.LogWarning(ex, "Не удалось получить данные из Redis по ключу {Key}", key);

                return default;
            }
        }

        /// <summary>
        /// Сохраняет объект в Redis с указанным временем жизни TTL.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);

                await _db.StringSetAsync(key, json, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось сохранить данные в Redis по ключу {Key}", key);
            }
        }

        /// <summary>
        /// Удаляет объект из Redis по ключу.
        /// </summary>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось удалить данные из Redis по ключу {Key}", key);
            }
        }
    }
}
