using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Constants;
using CleanArch.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using FluentAssertions;

namespace CleanArch.Application.Tests.Account.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    private readonly RegisterCommandHandler _handler;

    static RegisterCommandHandlerTests()
    {
        MappingConfig.Configure();
    }

    public RegisterCommandHandlerTests()
    {
        _mockIdentityService = new Mock<IIdentityService>();
        _mockContext = new Mock<IApplicationDbContext>();

        var mockMembersDbSet = new Mock<DbSet<Member>>();
        _mockContext.Setup(x => x.Members).Returns(mockMembersDbSet.Object);

        _mockTransaction = new Mock<IDbContextTransaction>();
        _mockContext
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockTransaction.Object);

        _handler = new RegisterCommandHandler(_mockIdentityService.Object, _mockContext.Object);
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

        var expectedUserId = Guid.NewGuid();
        var expectedUserDto = new UserDto
        {
            Id = expectedUserId,
            Email = command.Email,
            Gender = command.Gender,
            DateOfBirth = command.DateOfBirth.ToDateTime(TimeOnly.MinValue),
            DisplayName = command.DisplayName,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Token = "generated_token",
            RefreshToken = "refresh_token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(3),
        };

        _mockIdentityService
            .Setup(x =>
                x.CreateUserAsync(
                    command.Email.ToLower(),
                    command.Email,
                    command.Password,
                    command.DisplayName,
                    command.FirstName,
                    command.LastName,
                    null // imageUrl
                )
            )
            .ReturnsAsync((Result.Success(), expectedUserDto));

        _mockIdentityService
            .Setup(x => x.AddToRolesAsync(It.IsAny<Guid>(), new[] { UserRoles.Member }))
            .ReturnsAsync(Result.Success());

        _mockIdentityService.Setup(x => x.AddToRolesAsync(expectedUserId, new[] { "Member" })).ReturnsAsync(Result.Success());

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockIdentityService.Setup(x => x.GetUserByIdAsync(expectedUserId)).ReturnsAsync(expectedUserDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);

        _mockIdentityService.Verify(
            x =>
                x.CreateUserAsync(
                    command.Email.ToLower(),
                    command.Email,
                    command.Password,
                    command.DisplayName,
                    command.FirstName,
                    command.LastName,
                    null // imageUrl
                ),
            Times.Once
        );
        _mockIdentityService.Verify(x => x.AddToRolesAsync(expectedUserId, new[] { "Member" }), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            .Setup(x =>
                x.CreateUserAsync(
                    command.Email.ToLower(),
                    command.Email,
                    command.Password,
                    command.DisplayName,
                    command.FirstName,
                    command.LastName,
                    null // imageUrl
                )
            )
            .ReturnsAsync((Result.Failure(error), (UserDto)null!));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _mockIdentityService.Verify(
            x =>
                x.CreateUserAsync(
                    command.Email.ToLower(),
                    command.Email,
                    command.Password,
                    command.DisplayName,
                    command.FirstName,
                    command.LastName,
                    null // imageUrl
                ),
            Times.Once
        );
        _mockIdentityService.Verify(x => x.AddToRolesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _mockContext.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
