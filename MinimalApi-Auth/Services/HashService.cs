using System.Security.Cryptography;
using System.Text;
namespace MinimalApi_Auth.Services
{
    public class HashService
    {
        public string HashPassword(string password)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            var computedHash = HashPassword(password);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(storedHash));
        }
    }
}
