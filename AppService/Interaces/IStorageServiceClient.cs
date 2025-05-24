using AppService.Models;

namespace AppService.Interaces;

public interface IStorageServiceClient
{
    Task<UploadResponse> GetPreSignedUrlAsync(UploadRequest request);
    Task<bool> ValidateImageAsync(string imageId);
}