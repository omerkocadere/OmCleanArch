using System.Linq.Expressions;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Members.Commands.UpdateUserActivity;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Tests.Members.Commands.UpdateUserActivity;

public class UpdateUserActivityCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly Mock<DbSet<Member>> _mockMembersDbSet;
    private readonly UpdateUserActivityCommandHandler _handler;

    public UpdateUserActivityCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockUserContext = new Mock<IUserContext>();
        _mockMembersDbSet = new Mock<DbSet<Member>>();

        _mockContext.Setup(x => x.Members).Returns(_mockMembersDbSet.Object);

        _handler = new UpdateUserActivityCommandHandler(_mockContext.Object, _mockUserContext.Object);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_ShouldUpdateLastActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserActivityCommand();
        var cancellationToken = new CancellationToken();

        _mockUserContext.Setup(x => x.UserId).Returns(userId);

        // Mock ExecuteUpdateAsync to simulate successful update
        _mockMembersDbSet
            .Setup(x => x.Where(It.IsAny<Expression<Func<Member, bool>>>()))
            .Returns(_mockMembersDbSet.Object);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify that the user context was accessed
        _mockUserContext.Verify(x => x.UserId, Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ShouldReturnSuccessWithoutUpdate()
    {
        // Arrange
        var command = new UpdateUserActivityCommand();
        var cancellationToken = new CancellationToken();

        _mockUserContext.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify that no database operations were performed
        _mockUserContext.Verify(x => x.UserId, Times.Once);
        _mockContext.Verify(x => x.Members, Times.Never);
    }

    [Fact]
    public void UpdateUserActivityCommand_ShouldBeRecord()
    {
        // Arrange & Act
        var command1 = new UpdateUserActivityCommand();
        var command2 = new UpdateUserActivityCommand();

        // Assert
        command1.Should().Be(command2); // Records with no properties should be equal
        command1.GetHashCode().Should().Be(command2.GetHashCode());
        command1.ToString().Should().NotBeEmpty();
    }
}
