using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Users.Create;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Application.Tests.Users.Create;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ITokenProvider> _mockTokenProvider;
    private readonly CreateUserCommandHandler _handler;

    static CreateUserCommandHandlerTests()
    {
        MappingConfig.Configure();
    }

    public CreateUserCommandHandlerTests()
    {
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        _mockTokenProvider = new Mock<ITokenProvider>();
        _handler = new CreateUserCommandHandler(_mockUserManager.Object, _mockTokenProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenCommandIsValid()
    {
        var command = new CreateUserCommand
        {
            Email = "john@test.com",
            DisplayName = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Password = "ValidPassword123!",
            Gender = "Male",
            City = "Test City",
            Country = "Test Country",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
        };
        var expectedToken = "generated_token";

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Member")).ReturnsAsync(IdentityResult.Success);

        _mockTokenProvider.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(expectedToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be(expectedToken);
        result.Value.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "Member"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUserCreationFails()
    {
        var command = new CreateUserCommand
        {
            Email = "john@test.com",
            DisplayName = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Password = "WeakPassword",
            Gender = "Male",
            City = "Test City",
            Country = "Test Country",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
        };

        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak" },
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}
