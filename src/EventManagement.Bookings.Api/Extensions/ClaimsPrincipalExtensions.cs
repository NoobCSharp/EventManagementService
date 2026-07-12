using EventManagement.Bookings.Domain.Enums;
using System.Security.Claims;

namespace EventManagement.Bookings.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Получает идентификатор текущего пользователя из JWT.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(value))
                throw new UnauthorizedAccessException("Идентификатор пользователя не найден!");

            if (!Guid.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("Идентификатор пользователя не является допустимым!");

            return userId;
        }

        /// <summary>
        /// Получает роль пользователя.
        /// </summary>
        public static Role GetUserRole(this ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("Роль пользователя не найдена!");

            if (!Enum.TryParse<Role>(role, true, out var parsed))
                throw new UnauthorizedAccessException("Роль пользователя некорректна!");

            return parsed;
        }
    }
}
