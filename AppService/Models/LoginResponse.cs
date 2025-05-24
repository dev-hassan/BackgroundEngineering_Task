namespace AppService.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } 
        public DateTime ExpiresAt { get; set; }
    }
}
