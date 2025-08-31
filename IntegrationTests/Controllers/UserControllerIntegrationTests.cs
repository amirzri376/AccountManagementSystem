using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using AccountManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using AccountManagementSystem;
using AccountManagementSystem.Controllers;

namespace IntegrationTests.Controllers
{
    public class UserControllerIntegrationTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
    {

        [Fact]
        public async Task Register_ShouldCreateUserInRealDatabase()
        {
            // Arrange
            var client = _factory.CreateClient();
            var validRequest = new RegisterRequest
            {
                Username = "integrationtestuser",
                Email = "integrationtest@example.com",
                Password = "StrongPassword123!",
                FirstName = "Integration",
                LastName = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/user/register", validRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify user was actually created in database
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == validRequest.Username);
            Assert.NotNull(userInDb);
            Assert.Equal(validRequest.Email, userInDb.Email);
            Assert.Equal(validRequest.FirstName, userInDb.FirstName);
            Assert.Equal(validRequest.LastName, userInDb.LastName);

            // Cleanup
            CleanupDatabase();
        }
    }
}
