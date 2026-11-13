using System;
using System.Security.Cryptography;
using System.Text;

namespace SiteTrack.Core.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string rawPassword)
        {
            if (string.IsNullOrEmpty(rawPassword)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rawPassword);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool VerifyPassword(string rawPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(rawPassword) || string.IsNullOrEmpty(storedHash)) return false;
            string computed = HashPassword(rawPassword);
            return string.Equals(computed, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}

