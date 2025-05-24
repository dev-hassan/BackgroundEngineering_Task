namespace AppService.Models
{
    public class UploadResponse
    {
        public string UploadUrl { get; set; } 
        public string Token { get; set; } 
        public DateTime ExpiresAt { get; set; }
    }
}
