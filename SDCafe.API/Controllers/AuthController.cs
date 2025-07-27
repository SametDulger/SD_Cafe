using Microsoft.AspNetCore.Mvc;
using SDCafe.API.Models;
using SDCafe.API.Services;
using SDCafe.Business;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(IUserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Geçersiz e-posta veya şifre." });
            }

            var token = _jwtService.GenerateToken(user);
            var response = new LoginResponse
            {
                Token = token,
                UserName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };

            return Ok(response);
        }
    }
} 