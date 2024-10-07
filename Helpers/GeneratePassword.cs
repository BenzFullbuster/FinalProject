using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace FinalProject.Helpers
{
    public static class GeneratePassword
    {
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string HashPassword(string password, byte[] salt)
        {
            var hashPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt!,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashPassword;
        }

        public static bool VerifyPassword(string password, byte[] salt, string hashPassword)
        {
            var inputPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt!,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return inputPassword == hashPassword;
        }
    }
}
