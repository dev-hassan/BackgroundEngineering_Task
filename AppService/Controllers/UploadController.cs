using AppService.Interaces;
using AppService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IStorageServiceClient _storageClient;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IStorageServiceClient storageClient, ILogger<UploadController> logger)
        {
            _storageClient = storageClient;
            _logger = logger;
        }

        [HttpPost("request-url")]
        public async Task<IActionResult> RequestUploadUrl([FromBody] UploadRequest request)
        {
            try
            {
                _logger.LogInformation("Requesting upload URL for file: {FileName}", request.FileName);

                var response = await _storageClient.GetPreSignedUrlAsync(request);

                _logger.LogInformation("Upload URL generated successfully for file: {FileName}", request.FileName);
                return Ok(new ApiResponse<UploadResponse>
                {
                    Success = true,
                    Message = "Upload URL generated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upload URL for file: {FileName}", request.FileName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to generate upload URL"
                });
            }
        }
    }
}
