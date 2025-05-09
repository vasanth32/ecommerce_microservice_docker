using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly UserManager _userManager;
    private readonly JwtService _jwtService;

    public AuthController(UserManager userManager, JwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            // Register user and generate token synchronously
            var user = _userManager.Register(registerDto);
            var token = _jwtService.GenerateToken(user);

            // Create tasks for simulated operations
            var emailTask = SimulateSendWelcomeEmailAsync(user);
            var loggingTask = SimulateLogRegistrationAsync(user);

            // Run both tasks concurrently
            await Task.WhenAll(emailTask, loggingTask);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task SimulateSendWelcomeEmailAsync(User user)
    {
        await Task.Delay(2000); // Simulate network delay
        Console.WriteLine($"Welcome email sent to {user.Email} at {DateTime.UtcNow}");
    }

    private async Task SimulateLogRegistrationAsync(User user)
    {
        await Task.Delay(1000); // Simulate database write delay
        Console.WriteLine($"User registration logged for {user.Name} (ID: {user.Id}) at {DateTime.UtcNow}");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto loginDto)
    {
        var user = _userManager.GetUserByEmail(loginDto.Email);
        if (user == null || user.Password != loginDto.Password) // Note: In production, use proper password hashing
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = _userManager.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }
} 