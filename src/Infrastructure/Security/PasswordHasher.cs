using BCrypt.Net;
using GiveAID.Application.Common.Interfaces;

namespace GiveAID.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch (SaltParseException)
        {
            // Hash string is malformed — treated as invalid password
            return false;
        }
        catch (FormatException)
        {
            // Hash format is invalid — treated as invalid password
            return false;
        }
        // Unexpected exceptions (OutOfMemoryException, StackOverflowException, etc.)
        // propagate to the caller so they can be handled appropriately.
    }
}
