using global::StorageService.Models;

namespace StorageService.Interfaces;

public interface IFileStorageService
{
    UploadMetadata CreateUploadMetadata(PreSignedUrlRequest request);
    UploadMetadata? GetUploadMetadata(string token);
    Task<string> StoreFileAsync(UploadMetadata metadata, IFormFile file);
    bool ImageExists(string imageId);
    string? GetImagePath(string imageId);
}

