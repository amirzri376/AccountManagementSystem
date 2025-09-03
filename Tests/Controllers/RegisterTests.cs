using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccountManagementSystem.Controllers;
using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using AccountManagementSystem.Services;

public class UserControllerTests
{
    [Fact]
    public async Task Register_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        var validRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await controller.Register(validRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value!;
        var responseType = response.GetType();

        // Use reflection to access properties
        var messageProperty = responseType.GetProperty("message");
        var userIdProperty = responseType.GetProperty("userId");

        Assert.NotNull(messageProperty);
        Assert.NotNull(userIdProperty);

        var message = messageProperty!.GetValue(response) as string;
        var userId = userIdProperty!.GetValue(response);

        Assert.Equal("User registered successfully", message);
        Assert.NotNull(userId);

        var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Username == validRequest.Username);
        Assert.NotNull(userInDb);
        Assert.Equal(validRequest.Username, userInDb.Username);
        Assert.Equal(validRequest.Email, userInDb.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify(validRequest.Password, userInDb.PasswordHash));
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenInputIsInvalid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_InvalidInput")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        // Force model validation failure (not auto-performed in unit tests)
        controller.ModelState.AddModelError("Username", "The Username field is required.");
        controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");
        controller.ModelState.AddModelError("Password", "The Password field must be at least 6 characters long.");

        var invalidRequest = new RegisterRequest
        {
            Username = "", // Invalid: Username is required
            Email = "invalid-email", // Invalid: Email format is incorrect
            Password = "123", // Invalid: Password is too short
        };

        // Act
        var result = await controller.Register(invalidRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_DuplicateUsername")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        var existingUser = new User
        {
            Username = "testuser",
            Email = "existinguser@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var duplicateUsernameRequest = new RegisterRequest
        {
            Username = "testuser", // Duplicate username
            Email = "newuser@example.com",
            Password = "StrongPassword123!"
        };

        // Act
        var result = await controller.Register(duplicateUsernameRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_DuplicateEmail")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        var existingUser = new User
        {
            Username = "existinguser",
            Email = "testuser@example.com", // Duplicate email
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var duplicateEmailRequest = new RegisterRequest
        {
            Username = "newuser",
            Email = "testuser@example.com", // Duplicate email
            Password = "StrongPassword123!"
        };

        // Act
        var result = await controller.Register(duplicateEmailRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already exists", badRequestResult.Value);
    }

    private class ThrowingDbContext : ApplicationDbContext
    {
        public ThrowingDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Database error");
        }
    }

    [Fact]
    public async Task Register_ShouldReturnInternalServerError_WhenDatabaseErrorOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_DbError")
            .Options;

        await using var context = new ThrowingDbContext(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        var validRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await controller.Register(validRequest);

        // Assert
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalServerErrorResult.StatusCode);
        Assert.Equal("Database error occurred during registration", internalServerErrorResult.Value);
    }

    private class ThrowingDbContextForUnexpected : ApplicationDbContext
    {
        public ThrowingDbContextForUnexpected(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new Exception("Unexpected error");
        }
    }

    [Fact]
    public async Task Register_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_UnexpectedError")
            .Options;

        await using var context = new ThrowingDbContextForUnexpected(options);
        var mockConfiguration = new Mock<IConfiguration>();
        var mockEmailService = new Mock<IEmailService>();
        var mockReCaptchaService = new Mock<IReCaptchaService>();

        var controller = new UserController(context, mockConfiguration.Object, mockEmailService.Object, mockReCaptchaService.Object);

        var validRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await controller.Register(validRequest);

        // Assert
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalServerErrorResult.StatusCode);
        Assert.Equal("An unexpected error occurred during registration", internalServerErrorResult.Value);
    }
}