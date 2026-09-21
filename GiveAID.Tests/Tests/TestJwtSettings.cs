using GiveAID.Web.Helpers;

namespace GiveAID.Tests
{
    /// <summary>
    /// Stubs the <c>JwtSettings</c> config class so that <c>JwtHelper</c> reads
    /// values from <c>app.config</c> instead of requiring a live Web.config.
    /// All properties delegate directly to <c>JwtSettings</c>, which reads
    /// <c>appSettings</c> in GiveAID.Tests\app.config.
    /// </summary>
    /// <remarks>
    /// No extra code needed — JwtSettings already reads from ConfigurationManager.
    /// This file exists as a documentation anchor and to allow future test-specific
    /// overrides (e.g. very short expiry for token-expiry tests).
    /// </remarks>
    public static class TestJwtSettings
    {
        // These are the values in app.config — kept here so tests can assert against them.
        public const string TestSecret   = "TestSecretKeyThatIsAtLeast32CharactersLong!!";
        public const string TestIssuer    = "GiveAID.Test";
        public const string TestAudience  = "GiveAID.Test";
        public const int    TestExpiryMinutes = 60;
    }
}
