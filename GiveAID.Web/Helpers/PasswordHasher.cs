using System;
using BCrypt.Net;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Password hashing using the standard BCrypt.Net-Next library.
    /// Single, well-tested implementation — no custom crypto.
    /// 
    /// Format: $2a$(2-digit-cost)$(22-char-b64-salt)(31-char-b64-hash)
    /// </summary>
    public static class PasswordHasher
    {
        // 11 = standard cost (≈200ms per hash). Adjust upward on faster hardware.
        private const int WorkFactor = 11;

        /// <summary>
        /// Hash a plaintext password using BCrypt.
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verify a plaintext password against a stored BCrypt hash.
        /// Returns false on any error (malformed hash, exception, etc.) so
        /// invalid inputs never crash the request pipeline.
        /// </summary>
        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
