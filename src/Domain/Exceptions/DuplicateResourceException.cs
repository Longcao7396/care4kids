namespace GiveAID.Domain.Exceptions;

/// <summary>
/// Thrown when a create operation would result in a duplicate resource (e.g., duplicate email).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class DuplicateResourceException : Exception
{
    public DuplicateResourceException(string message) : base(message)
    {
    }
}
