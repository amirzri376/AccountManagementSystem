using System.Security.Claims;
using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                // Get current user ID from JWT token
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized("Admin not found");
                }

                // Parse user ID safely
                if (!int.TryParse(adminId, out int adminIdInt))
                {
                    return Unauthorized("Invalid admin ID");
                }

                // Get admin from database
                var admin = await _context.Users.FindAsync(adminIdInt);

                if (admin == null)
                {
                    return Unauthorized("Admin not found");
                }

                // Get all users for admin dashboard
                var allUsers = await _context.Users
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.Username,
                        email = u.Email,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        role = u.Role,
                        createdAt = u.CreatedAt,
                        lastLoginAt = u.LastLoginAt,
                        isActive = u.IsActive
                    })
                    .ToListAsync();

                // Return admin dashboard with user list
                return Ok(new
                {
                    message = "Welcome to Admin Dashboard!",
                    admin = new
                    {
                        id = admin.Id,
                        username = admin.Username,
                        email = admin.Email,
                        firstName = admin.FirstName,
                        lastName = admin.LastName,
                        role = admin.Role,
                        createdAt = admin.CreatedAt,
                        lastLoginAt = admin.LastLoginAt
                    },
                    dashboardData = new
                    {
                        totalLogins = 1,
                        accountAge = DateTime.UtcNow - admin.CreatedAt,
                        isActive = admin.IsActive,
                        totalUsers = allUsers.Count
                    },
                    users = allUsers // Include all users in admin dashboard
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to load admin dashboard");
            }
        }

        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            // Get current admin ID from JWT token
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized("Admin not found");
            }

            // Parse admin ID safely
            if (!int.TryParse(adminId, out int adminIdInt))
            {
                return Unauthorized("Invalid admin ID");
            }

            // Prevent admin from deactivating themselves
            if (id == adminIdInt)
            {
                return BadRequest("Cannot deactivate your own account");
            }

            // Find the user to toggle
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Toggle the active status
            user.IsActive = !user.IsActive;

            try
            {
                await _context.SaveChangesAsync();

                string action = user.IsActive ? "activated" : "deactivated";
                return Ok(new { 
                    message = $"User {user.Username} has been {action}",
                    userId = user.Id,
                    username = user.Username,
                    isActive = user.IsActive
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating user status");
            }
        }
    }
}
