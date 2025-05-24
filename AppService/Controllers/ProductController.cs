using AppService.Interaces;
using AppService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppService.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IStorageServiceClient _storageClient;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IStorageServiceClient storageClient,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _storageClient = storageClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Creating product: {ProductName} with image ID: {ImageId}",
                    request.Name, request.ImageId);

                // Validate image exists in storage service
                var imageExists = await _storageClient.ValidateImageAsync(request.ImageId);
                if (!imageExists)
                {
                    _logger.LogWarning("Image validation failed for image ID: {ImageId}", request.ImageId);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid image ID"
                    });
                }

                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    ImageId = request.ImageId
                };

                var createdProduct = _productService.CreateProduct(product);

                _logger.LogInformation("Product created successfully: {ProductId}", createdProduct.Id);
                return Ok(new ApiResponse<Product>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = createdProduct
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create product"
                });
            }
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _productService.GetAllProducts();
                return Ok(new ApiResponse<IEnumerable<Product>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve products"
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(string id)
        {
            try
            {
                var product = _productService.GetProduct(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<Product>
                {
                    Success = true,
                    Message = "Product retrieved successfully",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product: {ProductId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve product"
                });
            }
        }
    }
}