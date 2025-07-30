using CleanArch.Integration.Tests.Common;
using CleanArch.Integration.Tests.Infrastructure;

namespace CleanArch.Integration.Tests.Endpoints;

public class UsersEndpointsTests : BaseIntegrationTest
{
    public UsersEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        user.TryGetProperty("email", out var email).Should().BeTrue();
        email.GetString().Should().Be(command.Email);
        
        user.TryGetProperty("displayName", out var displayName).Should().BeTrue();
        displayName.GetString().Should().Be(command.DisplayName);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        
        // First user
        var firstCommand = new CreateUserCommand
        {
            Email = email,
            DisplayName = "First User",
            FirstName = "First",
            LastName = "User",
            Password = "SecurePassword123!"
        };

        await HttpClient.PostAsJsonAsync("/api/users", firstCommand);

        // Second user with same email
        var secondCommand = new CreateUserCommand
        {
            Email = email,
            DisplayName = "Second User",
            FirstName = "Second",
            LastName = "User",
            Password = "AnotherPassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", secondCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "invalid-email", // Invalid email format
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "123" // Too short
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithEmptyRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "", // Empty required field
            DisplayName = "",
            FirstName = "",
            LastName = "",
            Password = ""
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_WithAuthentication_ShouldReturnUsers()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        SetAuthorizationHeader(user);

        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var users = await response.Content.ReadAsStringAsync();
        users.Should().NotBeNullOrEmpty();
        users.Should().Contain(user.Email);
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
