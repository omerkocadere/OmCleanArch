using CleanArch.Application.Users.Create;

namespace CleanArch.Application.Tests.Users.Create;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
    }

    private Mock<DbSet<User>> SetupMockContext(List<User>? existingUsers = null)
    {
        existingUsers ??= [];
        var mockUsersDbSet = existingUsers.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mockUsersDbSet;
    }

    private static CreateUserCommand CreateValidCommand(string email = "test@example.com")
    {
        return new CreateUserCommand
        {
            Email = email,
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
        };
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var hashedPassword = "hashed_password123";

        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns(hashedPassword);
        var mockUsersDbSet = SetupMockContext();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);

        // Verify interactions
        _mockPasswordHasher.Verify(x => x.Hash(command.Password), Times.Once);
        mockUsersDbSet.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        var existingEmail = "existing@example.com";
        var command = CreateValidCommand(existingEmail);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = existingEmail,
            DisplayName = "Existing User",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "existinghashedpassword",
            Member = new Member
            {
                Id = Guid.NewGuid(),
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                DisplayName = "Existing User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country",
                User = null!,
            },
        };

        SetupMockContext([existingUser]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotUnique);

        // Verify no side effects occurred
        _mockPasswordHasher.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetCorrectUserProperties()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@example.com",
            DisplayName = "New Display Name",
            FirstName = "John",
            LastName = "Doe",
            Password = "securepassword",
        };

        var hashedPassword = "hashed_securepassword";
        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns(hashedPassword);

        var mockUsersDbSet = SetupMockContext();

        User? capturedUser = null;
        mockUsersDbSet.Setup(x => x.Add(It.IsAny<User>())).Callback<User>(user => capturedUser = user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        capturedUser.Should().NotBeNull();
        capturedUser!.Id.Should().NotBeEmpty();
        capturedUser.Email.Should().Be(command.Email);
        capturedUser.DisplayName.Should().Be(command.DisplayName);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddDomainEvent()
    {
        // Arrange
        var command = CreateValidCommand("event@example.com");
        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns("hashed_password");

        var mockUsersDbSet = SetupMockContext();

        User? capturedUser = null;
        mockUsersDbSet.Setup(x => x.Add(It.IsAny<User>())).Callback<User>(user => capturedUser = user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        capturedUser.Should().NotBeNull();
        capturedUser!.DomainEvents.Should().NotBeEmpty();
        capturedUser.DomainEvents.Should().HaveCount(1);

        var domainEvent = capturedUser.DomainEvents.First();
        domainEvent.Should().BeOfType<UserCreatedDomainEvent>();

        var userCreatedEvent = (UserCreatedDomainEvent)domainEvent;
        userCreatedEvent.User.Should().Be(capturedUser);
        userCreatedEvent.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenThroughToContext()
    {
        // Arrange
        var command = CreateValidCommand("cancel@example.com");
        var cancellationToken = new CancellationToken();

        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns("hashed_password");
        var mockUsersDbSet = SetupMockContext();

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify interactions
        _mockPasswordHasher.Verify(x => x.Hash(command.Password), Times.Once);
        mockUsersDbSet.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmailCaseInsensitiveMatch_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        var command = CreateValidCommand("TEST@EXAMPLE.COM"); // Uppercase email

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com", // Lowercase email
            DisplayName = "Existing User",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "existinghashedpassword",
            Member = new Member
            {
                Id = Guid.NewGuid(),
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                DisplayName = "Existing User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country",
                User = null!,
            },
        };

        SetupMockContext([existingUser]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotUnique);
    }

    [Fact]
    public void CreateUserCommand_ShouldSupportRecordFeatures()
    {
        // Arrange
        var command1 = new CreateUserCommand
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
        };

        // Act & Assert for 'with' expression (covers the <Clone>$ method)
        var command2 = command1 with
        {
            Email = "new@example.com",
        };

        command2.Should().NotBe(command1); // Should be different objects
        command2.Email.Should().Be("new@example.com");
        command2.DisplayName.Should().Be(command1.DisplayName); // Other properties should remain the same

        // Test record equality and hash codes
        var command3 = new CreateUserCommand
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
        };

        command1.Should().Be(command3); // Should be equal
        command1.GetHashCode().Should().Be(command3.GetHashCode());
        command1.ToString().Should().NotBeEmpty();
    }
}
