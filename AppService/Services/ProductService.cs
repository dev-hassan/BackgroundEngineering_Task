using AppService.Interaces;
using AppService.Models;

namespace AppService.Services;

public class ProductService : IProductService
{
    private readonly Dictionary<string, Product> _products = new();

    public Product CreateProduct(Product product)
    {
        _products[product.Id] = product;
        return product;
    }

    public Product? GetProduct(string id)
    {
        _products.TryGetValue(id, out var product);
        return product;
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _products.Values;
    }
}