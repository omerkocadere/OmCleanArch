using CleanArch.Integration.Tests.Common;
using CleanArch.Integration.Tests.Infrastructure;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Integration.Tests.Endpoints;

public class TodoItemsEndpointsTests : BaseIntegrationTest
{
    public TodoItemsEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTodoItem_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var todoList = await CreateTestTodoListAsync(user.Id);
        SetAuthorizationHeader(user);

        var command = new CreateTodoItemCommand
        {
            ListId = todoList.Id,
            Title = "Test Todo Item",
            Note = "Test note",
            Priority = PriorityLevel.Medium,
            UserId = user.Id,
            Description = "Test description",
            DueDate = DateTime.Today.AddDays(7),
            Labels = ["work", "important"]
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/todoitems", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var todoItem = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        todoItem.Should().NotBeNull();
        todoItem!.Title.Should().Be(command.Title);
        todoItem.Note.Should().Be(command.Note);
        todoItem.Priority.Should().Be(command.Priority);
        todoItem.Description.Should().Be(command.Description);
        todoItem.Labels.Should().BeEquivalentTo(command.Labels);
    }

    [Fact]
    public async Task CreateTodoItem_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        SetAuthorizationHeader(user);

        var command = new CreateTodoItemCommand
        {
            ListId = 999, // Non-existent list
            Title = "", // Empty title should fail validation
            UserId = user.Id
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/todoitems", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTodoItemsWithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var todoList = await CreateTestTodoListAsync(user.Id);
        SetAuthorizationHeader(user);

        // Create multiple todo items
        for (int i = 1; i <= 15; i++)
        {
            await CreateTodoItemAsync(todoList.Id, user.Id, $"Todo Item {i}");
        }

        // Act
        var response = await HttpClient.GetAsync($"/api/todoitems?ListId={todoList.Id}&PageNumber=1&PageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadAsStringAsync();
        result.Should().NotBeNullOrEmpty();
        
        // Verify pagination structure
        var jsonDoc = JsonDocument.Parse(result);
        jsonDoc.RootElement.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeLessThanOrEqualTo(10);
        
        jsonDoc.RootElement.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
        totalCount.GetInt32().Should().Be(15);
    }

    [Fact]
    public async Task GetTodoItemById_WithValidId_ShouldReturnTodoItem()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var todoList = await CreateTestTodoListAsync(user.Id);
        SetAuthorizationHeader(user);

        var todoItem = await CreateTodoItemAsync(todoList.Id, user.Id, "Test Todo Item");

        // Act
        var response = await HttpClient.GetAsync($"/api/todoitems/{todoItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(todoItem.Id);
        result.Title.Should().Be(todoItem.Title);
    }

    [Fact]
    public async Task GetTodoItemById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        SetAuthorizationHeader(user);

        // Act
        var response = await HttpClient.GetAsync("/api/todoitems/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodoItem_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var todoList = await CreateTestTodoListAsync(user.Id);
        SetAuthorizationHeader(user);

        var todoItem = await CreateTodoItemAsync(todoList.Id, user.Id, "Original Title");

        var updateCommand = new
        {
            Id = todoItem.Id,
            ListId = todoList.Id,
            Title = "Updated Title",
            Note = "Updated note",
            Priority = (int)PriorityLevel.High,
            Done = true
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/todoitems/{todoItem.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await HttpClient.GetAsync($"/api/todoitems/{todoItem.Id}");
        var updatedItem = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        updatedItem!.Title.Should().Be("Updated Title");
        updatedItem.Done.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTodoItem_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var todoList = await CreateTestTodoListAsync(user.Id);
        SetAuthorizationHeader(user);

        var todoItem = await CreateTodoItemAsync(todoList.Id, user.Id, "Todo to Delete");

        // Act
        var response = await HttpClient.DeleteAsync($"/api/todoitems/{todoItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await HttpClient.GetAsync($"/api/todoitems/{todoItem.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TodoItemEndpoints_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act & Assert
        var getResponse = await HttpClient.GetAsync("/api/todoitems");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var postResponse = await HttpClient.PostAsJsonAsync("/api/todoitems", new CreateTodoItemCommand
        {
            Title = "Test Todo",
            ListId = 1, // Use a default list ID
            UserId = Guid.NewGuid() // Use a default user ID
        });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var putResponse = await HttpClient.PutAsJsonAsync("/api/todoitems/1", new { });
        putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var deleteResponse = await HttpClient.DeleteAsync("/api/todoitems/1");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<TodoItem> CreateTodoItemAsync(int listId, Guid userId, string title)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var todoItem = new TodoItem
        {
            ListId = listId,
            Title = title,
            Note = "Test note",
            Priority = PriorityLevel.Medium,
            UserId = userId
        };

        context.TodoItems.Add(todoItem);
        await context.SaveChangesAsync();
        
        return todoItem;
    }
}
