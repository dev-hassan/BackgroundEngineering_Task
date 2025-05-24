using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AppService.Interaces;
using AppService.Models;

namespace AppService.Services;

public class StorageServiceClient : IStorageServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StorageServiceClient> _logger;
    private readonly IConfiguration _configuration;

    public StorageServiceClient(HttpClient httpClient, ILogger<StorageServiceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<UploadResponse> GetPreSignedUrlAsync(UploadRequest request)
    {
        try
        {
            // Create signature for the request
            var signature = CreateSignature(request);

            var preSignedRequest = new PreSignedUrlRequest
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                ProductId = request.ProductId,
                Signature = signature
            };

            var json = JsonSerializer.Serialize(preSignedRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/storage/presigned-url", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PreSignedUrlResponse>>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse?.Data == null)
                throw new Exception("Invalid response from storage service");

            return new UploadResponse
            {
                UploadUrl = apiResponse.Data.UploadUrl,
                Token = apiResponse.Data.Token,
                ExpiresAt = apiResponse.Data.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pre-signed URL");
            throw;
        }
    }

    public async Task<bool> ValidateImageAsync(string imageId)
    {
        try
        {
            var validationRequest = new ImageValidationRequest { ImageId = imageId };
            var json = JsonSerializer.Serialize(validationRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/storage/validate-image", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image: {ImageId}", imageId);
            return false;
        }
    }

    private string CreateSignature(UploadRequest request)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"];

        var payload = $"{request.FileName}|{request.ContentType}|{request.FileSize}|{request.ProductId}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}