namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Options for email-related behaviour that are injected via IOptions&lt;T&gt; pattern.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// If true, newly registered users must verify their email before logging in.
    /// Set to false in Development to skip email verification loop during testing.
    /// </summary>
    public bool RequireVerification { get; set; } = true;
}
