using CleanArch.Application.Users.Create;

namespace CleanArch.Application.Tests.Users.Create;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly IMapper _mapper;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
        };

        var hashedPassword = "hashed_password123";
        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns(hashedPassword);

        // Setup Users DbSet with no existing users
        var users = new List<User>();
        Mock<DbSet<User>> mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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

        // Verify password was hashed
        _mockPasswordHasher.Verify(x => x.Hash(command.Password), Times.Once);

        // Verify user was added to context
        mockUsersDbSet.Verify(x => x.Add(It.IsAny<User>()), Times.Once);

        // Verify SaveChangesAsync was called
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
        };

        // Setup Users DbSet with existing user
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            DisplayName = "Existing User",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "existinghashedpassword",
        };

        var users = new List<User> { existingUser };
        var mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotUnique);

        // Verify password hasher was not called
        _mockPasswordHasher.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);

        // Verify user was not added
        mockUsersDbSet.Verify(x => x.Add(It.IsAny<User>()), Times.Never);

        // Verify SaveChangesAsync was not called
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

        var users = new List<User>();
        var mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
        var command = new CreateUserCommand
        {
            Email = "event@example.com",
            DisplayName = "Event User",
            FirstName = "Event",
            LastName = "User",
            Password = "password123",
        };

        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns("hashed_password");

        var users = new List<User>();
        var mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
        var command = new CreateUserCommand
        {
            Email = "cancel@example.com",
            DisplayName = "Cancel User",
            FirstName = "Cancel",
            LastName = "User",
            Password = "password123",
        };

        var cancellationToken = new CancellationToken();
        _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns("hashed_password");

        var users = new List<User>();
        var mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify that the cancellation token was passed to SaveChangesAsync
        _mockContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmailCaseInsensitiveMatch_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "TEST@EXAMPLE.COM", // Uppercase email
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
        };

        // Setup Users DbSet with existing user with lowercase email
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com", // Lowercase email
            DisplayName = "Existing User",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "existinghashedpassword",
        };

        var users = new List<User> { existingUser };
        var mockUsersDbSet = users.BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotUnique);
    }
}
