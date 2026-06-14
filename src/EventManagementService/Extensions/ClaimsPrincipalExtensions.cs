using Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventManagementService.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(value))
                throw new UnauthorizedAccessException("Идентификатор пользователя не найден!");

            if (!Guid.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("Идентификатор пользователя не является допустимым!");

            return userId;
        }

        public static Role GetUserRole(this ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("Роль пользователя не найдена!");

            return Enum.TryParse<Role>(role, true, out var parsed) ? parsed : Role.User;
        }
    }
}
