using System;
using BCrypt.Net;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// One-shot BCrypt password reset utility invoked by AuthInitController.
    /// Updates the admin + demo accounts with correctly-formatted BCrypt hashes
    /// for "Admin@123" / "User@123". Idempotent.
    /// </summary>
    internal static class AuthBootstrap
    {
        public static string Hash(string password, int workFactor)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }
}
