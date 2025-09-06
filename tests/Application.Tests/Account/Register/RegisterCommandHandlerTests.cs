using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Tests.Account.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<ITokenProvider> _mockTokenProvider;
    private readonly RegisterCommandHandler _handler;

    static RegisterCommandHandlerTests()
    {
        MappingConfig.Configure();
    }

    public RegisterCommandHandlerTests()
    {
        _mockIdentityService = new Mock<IIdentityService>();
        _mockTokenProvider = new Mock<ITokenProvider>();
        _handler = new RegisterCommandHandler(_mockIdentityService.Object, _mockTokenProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenCommandIsValid()
    {
        var command = new RegisterCommand
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
        
        var expectedUserDto = new UserDto
        {
            Email = command.Email,
            DisplayName = command.DisplayName,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Token = "generated_token",
            RefreshToken = "refresh_token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(3)
        };

        _mockIdentityService
            .Setup(x => x.CreateUserAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(Result.Success());

        _mockIdentityService.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Member")).ReturnsAsync(Result.Success());

        _mockTokenProvider
            .Setup(x => x.CreateUserWithTokensAsync(It.IsAny<User>(), false))
            .ReturnsAsync(Result.Success(expectedUserDto));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);

        _mockIdentityService.Verify(x => x.CreateUserAsync(It.IsAny<User>(), command.Password), Times.Once);
        _mockIdentityService.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "Member"), Times.Once);
        _mockTokenProvider.Verify(x => x.CreateUserWithTokensAsync(It.IsAny<User>(), false), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUserCreationFails()
    {
        var command = new RegisterCommand
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

        var error = Error.Validation("PasswordTooWeak", "Password is too weak");

        _mockIdentityService
            .Setup(x => x.CreateUserAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(Result.Failure(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _mockIdentityService.Verify(x => x.CreateUserAsync(It.IsAny<User>(), command.Password), Times.Once);
        _mockIdentityService.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}
