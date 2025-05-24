namespace AppService.Models
{
    public class UploadRequest
    {
        public string FileName { get; set; } 
        public string ContentType { get; set; } 
        public long FileSize { get; set; }
        public string ProductId { get; set; } 
    }



   

    
}
