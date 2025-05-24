namespace AppService.Models
{
    public class CreateProductRequest
    {
        public string Name { get; set; } 
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public string ImageId { get; set; } 
    }
}
