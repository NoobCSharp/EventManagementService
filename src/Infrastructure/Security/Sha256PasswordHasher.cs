using Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security
{
    public sealed class Sha256PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            return Convert.ToHexString(bytes);
        }

        public bool Verify(string password, string passwordHash)
        {
            if (passwordHash is null)
                throw new ArgumentNullException(nameof(passwordHash));

            string computedHash = Hash(password);

            return string.Equals(computedHash, passwordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
