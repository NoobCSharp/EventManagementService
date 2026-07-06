using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManagement.Identity.Infrastructure.Security
{
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtGenerationOptions _options;

        public JwtTokenGenerator(IOptions<JwtGenerationOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(Guid userId, string login, Role role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
