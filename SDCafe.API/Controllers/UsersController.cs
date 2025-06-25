using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByRole(UserRole role)
        {
            var users = await _userService.GetByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("waiters")]
        public async Task<ActionResult<IEnumerable<User>>> GetWaiters()
        {
            var waiters = await _userService.GetByRoleAsync(UserRole.Waiter);
            return Ok(waiters);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
        {
            if (!await _userService.IsEmailUniqueAsync(request.Email))
            {
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber,
                CreatedDate = DateTime.Now
            };

            var createdUser = await _userService.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _userService.IsEmailUniqueAsync(request.Email, id))
            {
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Role = request.Role;
            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedDate = DateTime.Now;

            await _userService.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            user.UpdatedDate = DateTime.Now;
            await _userService.UpdateAsync(user);
            return NoContent();
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var success = await _userService.UpdatePasswordAsync(id, request.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Parola değiştirilirken hata oluştu." });
            }

            return NoContent();
        }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserRole Role { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = null!;
    }
} 