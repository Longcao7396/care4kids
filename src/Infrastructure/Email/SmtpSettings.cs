namespace GiveAID.Infrastructure.Email;

public class SmtpSettings
{
    public bool SmtpEnabled { get; set; } = false;
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@give-aid.org";
    public string FromName { get; set; } = "GiveAID";
    public string PublicSiteUrl { get; set; } = "https://giveaid.org";
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}
