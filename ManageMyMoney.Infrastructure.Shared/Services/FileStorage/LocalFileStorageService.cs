using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManageMyMoney.Infrastructure.Shared.Services.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings, ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        EnsureDirectoryExists();
    }

    public async Task<OperationResult<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_settings.AllowedExtensions.Contains(extension))
            {
                return OperationResult.Failure<string>($"File extension '{extension}' is not allowed");
            }

            if (fileStream.Length > _settings.MaxFileSizeMB * 1024 * 1024)
            {
                return OperationResult.Failure<string>($"File size exceeds maximum of {_settings.MaxFileSizeMB}MB");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_settings.StoragePath, uniqueFileName);

            await using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);

            var fileUrl = GetFileUrl(uniqueFileName);
            _logger.LogInformation("File uploaded: {FileName} -> {FileUrl}", fileName, fileUrl);

            return OperationResult.Success(fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return OperationResult.Failure<string>($"Error uploading file: {ex.Message}");
        }
    }

    public async Task<OperationResult<Stream>> DownloadFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_settings.StoragePath, fileName);

            if (!File.Exists(filePath))
            {
                return OperationResult.Failure<Stream>("File not found");
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await Task.FromResult(OperationResult.Success<Stream>(stream));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
            return OperationResult.Failure<Stream>($"Error downloading file: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_settings.StoragePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileUrl}", fileUrl);
            }

            return await Task.FromResult(OperationResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return OperationResult.Failure($"Error deleting file: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> FileExistsAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_settings.StoragePath, fileName);
            var exists = File.Exists(filePath);
            return await Task.FromResult(OperationResult.Success(exists));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return OperationResult.Failure<bool>($"Error checking file: {ex.Message}");
        }
    }

    public string GetFileUrl(string fileName)
    {
        return string.IsNullOrEmpty(_settings.BaseUrl)
            ? $"/files/{fileName}"
            : $"{_settings.BaseUrl.TrimEnd('/')}/files/{fileName}";
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_settings.StoragePath))
        {
            Directory.CreateDirectory(_settings.StoragePath);
        }
    }
}
