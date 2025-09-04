using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Members.Commands.UpdateUserActivity;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Tests.Members.Commands.UpdateUserActivity;

public class UpdateUserActivityCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Member>> _mockMembersDbSet;
    private readonly UpdateUserActivityCommandHandler _handler;

    public UpdateUserActivityCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMembersDbSet = new Mock<DbSet<Member>>();

        _mockContext.Setup(x => x.Members).Returns(_mockMembersDbSet.Object);

        _handler = new UpdateUserActivityCommandHandler(_mockContext.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateHandlerSuccessfully()
    {
        // Arrange & Act & Assert
        _handler.Should().NotBeNull();
        _mockContext.Verify(x => x.Members, Times.Never); // Constructor shouldn't access Members
    }

    [Fact]
    public void UpdateUserActivityCommand_ShouldBeRecord()
    {
        // Arrange & Act
        var userId = Guid.NewGuid();
        var command1 = new UpdateUserActivityCommand(userId);
        var command2 = new UpdateUserActivityCommand(userId);
        var command3 = new UpdateUserActivityCommand(Guid.NewGuid());

        // Assert
        command1.Should().Be(command2); // Records with same values should be equal
        command1.Should().NotBe(command3); // Records with different values should not be equal
        command1.GetHashCode().Should().Be(command2.GetHashCode());
        command1.ToString().Should().NotBeEmpty();
        command1.UserId.Should().Be(userId);
    }

    [Fact]
    public void UpdateUserActivityCommand_ShouldHaveCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var command = new UpdateUserActivityCommand(userId);

        // Assert
        command.UserId.Should().Be(userId);
    }
}
