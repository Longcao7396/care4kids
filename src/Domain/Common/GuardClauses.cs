using System;

namespace GiveAID.Domain.Common
{
    /// <summary>
    /// Guard clauses for validating domain invariants.
    /// Throws ArgumentException when business rules are violated.
    /// </summary>
    public static class GuardClauses
    {
        public static void AgainstNull<T>(T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        public static void AgainstNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        }

        public static void AgainstNegative(decimal value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} cannot be negative.", paramName);
        }

        public static void AgainstZero(decimal value, string paramName)
        {
            if (value == 0)
                throw new ArgumentException($"{paramName} cannot be zero.", paramName);
        }

        public static void AgainstNegativeOrZero(decimal value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero.", paramName);
        }

        public static void AgainstFutureDate(DateTime value, string paramName)
        {
            if (value > DateTime.UtcNow)
                throw new ArgumentException($"{paramName} cannot be a future date.", paramName);
        }

        public static void AgainstPastDate(DateTime value, string paramName)
        {
            if (value < DateTime.UtcNow)
                throw new ArgumentException($"{paramName} cannot be a past date.", paramName);
        }
    }
}
