using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Geçersiz e-posta veya şifre." });
            }

            return Ok(new LoginResponse
            {
                User = user,
                Token = GenerateJwtToken(user),
                Message = "Giriş başarılı"
            });
        }

        [HttpGet("profile")]
        public async Task<ActionResult<User>> GetProfile()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UpdatedDate = DateTime.Now;

            await _userService.UpdateAsync(user);
            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userService.AuthenticateAsync(userEmail, request.CurrentPassword);
            if (currentUser == null)
            {
                return BadRequest(new { message = "Mevcut şifre yanlış." });
            }

            // UpdatePasswordAsync metodunu kullan - bu çift hash'leme problemini önler
            var success = await _userService.UpdatePasswordAsync(user.Id, request.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Parola değiştirilirken hata oluştu." });
            }

            return NoContent();
        }

        private string GenerateJwtToken(User user)
        {
            return $"jwt_token_for_user_{user.Id}";
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public User User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
} 