namespace ManageMyMoney.Infrastructure.Shared.Settings;

public record FileStorageSettings
{
    public string StoragePath { get; init; } = "Storage/Files";
    public int MaxFileSizeMB { get; init; } = 10;
    public string[] AllowedExtensions { get; init; } = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    public string BaseUrl { get; init; } = string.Empty;
}
