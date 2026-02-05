using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<OperationResult<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<OperationResult<Stream>> DownloadFileAsync(string fileUrl);
    Task<OperationResult> DeleteFileAsync(string fileUrl);
    Task<OperationResult<bool>> FileExistsAsync(string fileUrl);
    string GetFileUrl(string fileName);
}
