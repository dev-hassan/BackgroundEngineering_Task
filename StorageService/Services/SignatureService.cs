using System.Security.Cryptography;
using System.Text;
using StorageService.Interfaces;
using StorageService.Models;

namespace StorageService.Services;

public class SignatureService : ISignatureService
{
    private readonly IConfiguration _configuration;

    public SignatureService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool VerifySignature(PreSignedUrlRequest request)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = jwtSettings["SecretKey"];

            var payload = $"{request.FileName}|{request.ContentType}|{request.FileSize}|{request.ProductId}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            return expectedSignature == request.Signature;
        }
        catch
        {
            return false;
        }
    }
}