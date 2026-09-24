using System;
using System.Text.RegularExpressions;

namespace GiveAID.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a validated email address.
    /// </summary>
    public sealed class EmailAddress
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email address cannot be null or empty.", nameof(value));

            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public override bool Equals(object? obj)
        {
            if (obj is EmailAddress other)
                return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode()
        {
            return Value.ToLowerInvariant().GetHashCode();
        }

        public override string ToString() => Value;
    }
}
