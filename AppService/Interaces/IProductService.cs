using AppService.Models;

namespace AppService.Interaces;

public interface IProductService
{
    Product CreateProduct(Product product);
    Product? GetProduct(string id);
    IEnumerable<Product> GetAllProducts();
}