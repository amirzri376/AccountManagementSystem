using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AccountManagementSystem.Data;
using AccountManagementSystem.Services;
using Microsoft.Extensions.Configuration;
using AccountManagementSystem;

namespace IntegrationTests.Controllers
{
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly ApplicationDbContext _context;

        protected IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Test");
                builder.UseSetting("ASPNETCORE_STATICWEBASSETS", "false");
                builder.UseSetting("ASPNETCORE_CONTENTROOT", Directory.GetCurrentDirectory());
                builder.UseSetting("ASPNETCORE_WEBROOT", Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Only load appsettings.Test.json if NOT running in Docker
                    // In Docker, environment variables should take precedence
                    if (!Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        config.AddJsonFile("appsettings.Test.json", optional: false);
                    }
                });
            });
            
            // Get the database context from the application
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        protected void CleanupDatabase()
        {
            // Clean up test data after each test
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }
    }
}
