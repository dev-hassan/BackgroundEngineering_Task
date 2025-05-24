using Microsoft.AspNetCore.Mvc;
using StorageService.Interfaces;
using StorageService.Models;
using StorageService.Services;

namespace StorageService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ISignatureService _signatureService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IFileStorageService fileStorageService,
        ISignatureService signatureService,
        ILogger<StorageController> logger)
    {
        _fileStorageService = fileStorageService;
        _signatureService = signatureService;
        _logger = logger;
    }

    [HttpPost("presigned-url")]
    public IActionResult GeneratePreSignedUrl([FromBody] PreSignedUrlRequest request)
    {
        try
        {
            _logger.LogInformation("Generating pre-signed URL for file: {FileName}", request.FileName);

            // Verify signature
            if (!_signatureService.VerifySignature(request))
            {
                _logger.LogWarning("Invalid signature for file: {FileName}", request.FileName);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid signature"
                });
            }

            // Generate pre-signed URL and token
            var metadata = _fileStorageService.CreateUploadMetadata(request);
            var uploadUrl = $"{Request.Scheme}://{Request.Host}/api/storage/upload/{metadata.Id}";

            var response = new PreSignedUrlResponse
            {
                UploadUrl = uploadUrl,
                Token = metadata.Id,
                ExpiresAt = metadata.ExpiresAt
            };

            _logger.LogInformation("Pre-signed URL generated successfully for file: {FileName}", request.FileName);
            return Ok(new ApiResponse<PreSignedUrlResponse>
            {
                Success = true,
                Message = "Pre-signed URL generated successfully",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URL for file: {FileName}", request.FileName);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to generate pre-signed URL"
            });
        }
    }

    [HttpPost("upload/{token}")]
    [RequestSizeLimit(100_000_000)] // 100MB limit
    public async Task<IActionResult> UploadFile(string token, IFormFile file)
    {
        try
        {
            _logger.LogInformation("Uploading file with token: {Token}", token);

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            // Validate upload token and metadata
            var metadata = _fileStorageService.GetUploadMetadata(token);
            if (metadata == null)
            {
                _logger.LogWarning("Invalid upload token: {Token}", token);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid upload token"
                });
            }

            if (metadata.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired upload token: {Token}", token);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Upload token has expired"
                });
            }

            // Validate file matches metadata
            if (file.FileName != metadata.FileName ||
                file.ContentType != metadata.ContentType ||
                file.Length != metadata.FileSize)
            {
                _logger.LogWarning("File metadata mismatch for token: {Token}", token);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "File metadata does not match original request"
                });
            }

            // Store the file
            var imageId = await _fileStorageService.StoreFileAsync(metadata, file);

            var response = new UploadResponse
            {
                ImageId = imageId,
                Message = "File uploaded successfully"
            };

            _logger.LogInformation("File uploaded successfully with image ID: {ImageId}", imageId);
            return Ok(new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "File uploaded successfully",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file with token: {Token}", token);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to upload file"
            });
        }
    }

    [HttpPost("validate-image")]
    public IActionResult ValidateImage([FromBody] ImageValidationRequest request)
    {
        try
        {
            _logger.LogInformation("Validating image: {ImageId}", request.ImageId);

            var isValid = _fileStorageService.ImageExists(request.ImageId);

            if (isValid)
            {
                _logger.LogInformation("Image validation successful: {ImageId}", request.ImageId);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Image exists"
                });
            }
            else
            {
                _logger.LogWarning("Image validation failed: {ImageId}", request.ImageId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Image not found"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image: {ImageId}", request.ImageId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to validate image"
            });
        }
    }

    [HttpGet("image/{imageId}")]
    public IActionResult GetImage(string imageId)
    {
        try
        {
            var filePath = _fileStorageService.GetImagePath(imageId);
            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image: {ImageId}", imageId);
            return StatusCode(500);
        }
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}