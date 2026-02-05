namespace ManageMyMoney.Infrastructure.Shared.Settings;

public record EmailSettings
{
    public string SmtpServer { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public string TemplatesPath { get; init; } = "Templates/Email";
}
