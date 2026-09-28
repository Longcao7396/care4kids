namespace GiveAID.Application.Services
{
    /// <summary>
    /// Service for retrieving the current authenticated user's ID.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user's ID from the authentication context.
        /// Returns null if no user is authenticated.
        /// </summary>
        string? GetUserId();
    }
}
