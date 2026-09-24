namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Interface for password hashing operations.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    bool Verify(string password, string hash);
}
