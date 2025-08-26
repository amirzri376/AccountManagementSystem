using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using AccountManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace AccountManagementSystem.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserController(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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

            try
            {
                // Save to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully", userId = user.Id });
            }
            catch (DbUpdateException)
            {
                // Handle database connection errors or constraint violations
                return StatusCode(500, "Database error occurred during registration");
            }
            catch (Exception)
            {
                // Handle other unexpected errors
                return StatusCode(500, "An unexpected error occurred during registration");
            }
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

            try
            {
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
                        email = user.Email,
                        role = user.Role // Include role in login response
                    }
                });
            }
            catch (DbUpdateException)
            {
                // Handle database errors during login update
                return StatusCode(500, "Database error occurred during login");
            }
            catch (Exception)
            {
                // Handle other unexpected errors
                return StatusCode(500, "An unexpected error occurred during login");
            }
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(GetValidationErrorMessage());
            }
            // Check if email exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user != null)
            {
                // Generate secure reset token
                var resetToken = GenerateResetToken();

                // Store token in database
                user.ResetToken = resetToken;
                user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Expires in 1 hour
                user.ResetTokenUsed = false;
                await _context.SaveChangesAsync();

                // Create reset link
                var appUrl = _configuration["AppUrl"] ?? "https://localhost:7032";
                var resetLink = $"{appUrl}/reset-password?token={resetToken}";

                // Send email using our email service
                var subject = "Reset Your Password";
                var body = $"Click the following link to reset your password: {resetLink}";

                await _emailService.SendEmailAsync(request.Email, subject, body);
            }

            // Always return the same message for security
            return Ok(new { message = "If an account with this email exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(GetValidationErrorMessage());
            }

            // Find user by reset token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == request.Token);

            if (user == null)
            {
                return BadRequest("Invalid or expired reset token");
            }

            // Check if token is expired
            if (user.ResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Reset token has expired");
            }

            // Check if token has already been used
            if (user.ResetTokenUsed)
            {
                return BadRequest("Reset token has already been used");
            }

            try
            {
                // Hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Update user's password
                user.PasswordHash = hashedPassword;

                // Mark token as used
                user.ResetTokenUsed = true;
                user.ResetToken = null; // Clear the token
                user.ResetTokenExpiry = null; // Clear expiry

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while resetting the password");
            }
        }


        [Authorize(Roles = "User")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Get current user ID from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            // Parse user ID safely
            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized("Invalid user ID");
            }

            // Get user from database
            var user = await _context.Users.FindAsync(userIdInt);

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
                    role = user.Role, // Include role in response
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
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims are pieces of information about the user stored in the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Add role to JWT token
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

        private static string GetFieldDisplayName(string fieldName)
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

        private static string GenerateResetToken()
        {
            // Generate a cryptographically secure random token
            var randomBytes = new byte[32]; // 256 bits
            System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);

            // Convert to URL-safe string (32 characters)
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")[..32];
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
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}