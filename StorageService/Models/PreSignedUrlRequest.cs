namespace StorageService.Models;

public class PreSignedUrlRequest
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string Signature { get; set; }
    public string ProductId { get; set; }
}


