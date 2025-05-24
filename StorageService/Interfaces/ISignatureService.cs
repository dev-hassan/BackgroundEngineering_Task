using global::StorageService.Models;

namespace StorageService.Interfaces;

public interface ISignatureService
{
    bool VerifySignature(PreSignedUrlRequest request);
}
