using StorageService.Interfaces;
using StorageService.Models;

namespace StorageService.Services;
public class FileStorageService : IFileStorageService
{
    private readonly Dictionary<string, UploadMetadata> _uploadMetadata = new();
    private readonly Dictionary<string, string> _imageStorage = new();
    private readonly string _uploadPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public UploadMetadata CreateUploadMetadata(PreSignedUrlRequest request)
    {
        var metadata = new UploadMetadata
        {
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            ProductId = request.ProductId,
            Signature = request.Signature,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15) // 15 minutes to upload
        };

        _uploadMetadata[metadata.Id] = metadata;

        _logger.LogInformation("Created upload metadata with ID: {MetadataId} for file: {FileName}",
            metadata.Id, metadata.FileName);

        return metadata;
    }

    public UploadMetadata? GetUploadMetadata(string token)
    {
        _uploadMetadata.TryGetValue(token, out var metadata);
        return metadata;
    }

    public async Task<string> StoreFileAsync(UploadMetadata metadata, IFormFile file)
    {
        try
        {
            var imageId = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(metadata.FileName);
            var fileName = $"{imageId}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update metadata
            metadata.IsUploaded = true;
            metadata.FilePath = filePath;

            // Store image reference
            _imageStorage[imageId] = filePath;

            _logger.LogInformation("File stored successfully with image ID: {ImageId} at path: {FilePath}",
                imageId, filePath);

            return imageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing file for metadata ID: {MetadataId}", metadata.Id);
            throw;
        }
    }

    public bool ImageExists(string imageId)
    {
        if (!_imageStorage.TryGetValue(imageId, out var filePath))
        {
            return false;
        }

        return File.Exists(filePath);
    }

    public string? GetImagePath(string imageId)
    {
        _imageStorage.TryGetValue(imageId, out var filePath);
        return filePath;
    }
}

