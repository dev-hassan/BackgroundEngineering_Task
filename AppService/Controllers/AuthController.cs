using AppService.Interaces;
using Microsoft.AspNetCore.Mvc;
using AppService.Models;

namespace AppService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Simple authentication (in production, use proper user management)
        if (request.Username == "seller" && request.Password == "password123")
        {
            var token = _tokenService.GenerateToken(request.Username);
            var response = new LoginResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _logger.LogInformation("Login successful for user: {Username}", request.Username);
            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            });
        }

        _logger.LogWarning("Login failed for user: {Username}", request.Username);
        return Unauthorized(new ApiResponse<object>
        {
            Success = false,
            Message = "Invalid credentials"
        });
    }
}


