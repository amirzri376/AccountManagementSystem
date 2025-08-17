using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using LoginSystem.Data;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginSystem.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(GetValidationErrorMessage());
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists");
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(GetValidationErrorMessage());
            }

            // Find user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return BadRequest("Invalid username or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return BadRequest("Account is deactivated");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Invalid username or password");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email
                }
            });
        }

        [Authorize]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Get current user ID from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            // Get user from database
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Return dashboard data
            return Ok(new
            {
                message = "Welcome to your dashboard!",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLoginAt
                },
                dashboardData = new
                {
                    totalLogins = 1,
                    accountAge = DateTime.UtcNow - user.CreatedAt,
                    isActive = user.IsActive
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetValidationErrorMessage()
        {
            var errorMessages = new List<string>();
            var processedFields = new HashSet<string>();
            
            foreach (var modelStateEntry in ModelState)
            {
                var fieldName = modelStateEntry.Key;
                var modelState = modelStateEntry.Value;
                
                // Skip if we already processed this field (to avoid duplicate errors)
                if (processedFields.Contains(fieldName))
                    continue;
                
                foreach (var error in modelState.Errors)
                {
                    // Convert technical error messages to user-friendly ones with field name
                    var userMessage = ConvertToUserFriendlyMessage(error.ErrorMessage, fieldName);
                    errorMessages.Add(userMessage);
                    processedFields.Add(fieldName);
                    break; // Only take the first error per field to avoid duplicates
                }
            }
            return string.Join(", ", errorMessages);
        }

        private string ConvertToUserFriendlyMessage(string technicalMessage, string fieldName)
        {
            // Convert technical validation messages to user-friendly ones with field name
            return technicalMessage switch
            {
                var msg when msg.Contains("required") 
                    => $"{GetFieldDisplayName(fieldName)} is required",
                var msg when msg.Contains("Username") && msg.Contains("maximum length") 
                    => "Username must be 50 characters or less",
                var msg when msg.Contains("Email") && (msg.Contains("valid e-mail") || msg.Contains("valid email"))
                    => "Please enter a valid email address",
                var msg when msg.Contains("Password") && msg.Contains("maximum length") 
                    => "Password must be 100 characters or less",
                var msg when msg.Contains("FirstName") && msg.Contains("maximum length") 
                    => "First name must be 50 characters or less",
                var msg when msg.Contains("LastName") && msg.Contains("maximum length") 
                    => "Last name must be 50 characters or less",
                _ => technicalMessage // Keep original if no specific conversion
            };
        }

        private string GetFieldDisplayName(string fieldName)
        {
            return fieldName switch
            {
                "Username" => "Username",
                "Email" => "Email",
                "Password" => "Password",
                "FirstName" => "First name",
                "LastName" => "Last name",
                _ => fieldName
            };
        }
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }
    }

      public class LoginRequest
  {
      [Required]
      [StringLength(50)]
      public string Username { get; set; } = string.Empty;
      
      [Required]
      [StringLength(100)]
      public string Password { get; set; } = string.Empty;
  }
}