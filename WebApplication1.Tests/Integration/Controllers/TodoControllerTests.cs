using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Models;
using Bogus;

namespace WebApplication1.Tests.Integration.Controllers;

public class TodoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TodoControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Todos.RemoveRange(db.Todos);
        db.SaveChanges();
        GC.SuppressFinalize(this);
    }

    private static readonly Faker<Todo> _todoFaker = new Faker<Todo>()
        .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
        .RuleFor(t => t.Description, f => f.Lorem.Sentence(6))
        .RuleFor(t => t.Done, f => f.Random.Bool());

    private List<Todo> SeedTodos(int count, bool doneRandom = true)
    {
        var todos = doneRandom
            ? _todoFaker.Generate(count)
            : new Faker<Todo>()
                .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(6))
                .RuleFor(t => t.Done, f => false)
                .Generate(count);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            db.Todos.AddRange(todos);
            db.SaveChanges();
        }
        return todos;
    }

    [Fact]
    public async Task Create_AddsTodoAndReturnsCreated()
    {
        // Arrange
        var newTodo = new Todo { Subject = "Test Subject", Description = "Test Description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Todo", newTodo);
        var created = await response.Content.ReadFromJsonAsync<Todo>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(newTodo.Subject, created!.Subject);
        Assert.Equal(newTodo.Description, created.Description);
        Assert.False(created.Done);
        Assert.True(created.Id > 0);
        Assert.True(created.CreatedAt > DateTime.MinValue);
        Assert.True(created.UpdatedAt > DateTime.MinValue);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task Create_ReturnsUnprocessableEntity_WhenSubjectIsMissing()
    {
        // Arrange
        var newTodo = new Todo { Done = true };
        // Act
        var response = await _client.PostAsJsonAsync("/api/Todo", newTodo);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Subject", content);
        Assert.Contains("Description", content);
    }

    [Fact]
    public async Task Create_ReturnsUnprocessableEntity_WhenDescriptionIsMissing()
    {
        // Arrange
        var newTodo = new Todo { Subject = "Test subject" };
        // Act
        var response = await _client.PostAsJsonAsync("/api/Todo", newTodo);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Description", content);
    }

    [Fact]
    public async Task Update_ChangesTodoAndReturnsOk()
    {
        // Arrange
        var todo = SeedTodos(1, false)[0];
        // Act
        var update = new Todo { Subject = todo.Subject, Description = todo.Description, Done = true };
        var response = await _client.PutAsJsonAsync($"/api/Todo/{todo.Id}", update);
        var updated = await response.Content.ReadFromJsonAsync<Todo>();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(update.Subject, updated!.Subject);
        Assert.Equal(update.Description, updated.Description);
        Assert.True(updated.Done);
        Assert.Equal(todo.Id, updated.Id);
        Assert.True(updated.UpdatedAt > todo.UpdatedAt);
    }

    [Fact]
    public async Task Delete_RemovesTodoAndReturnsNoContent()
    {
        var todo = SeedTodos(1)[0];
        var response = await _client.DeleteAsync($"/api/Todo/{todo.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedTodos()
    {
        var todos = SeedTodos(15);
        var response = await _client.GetAsync("/api/Todo?page=2&pageSize=5");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Todo>>();
        Assert.NotNull(result);
        Assert.Equal(15, result!.total);
        Assert.Equal(5, result.items.Count);
        Assert.Equal(6, result.from);
        Assert.Equal(10, result.to);
        Assert.Equal(2, result.current_page);
        Assert.Equal(3, result.total_page);
        Assert.Equal(3, result.last_page);
    }

    [Fact]
    public async Task GetAll_ReturnsAllTodos()
    {
        var todos = SeedTodos(3);
        var response = await _client.GetAsync("/api/Todo");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Todo>>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.items.Count);
        foreach (var todo in todos)
        {
            Assert.Contains(result.items, t => t.Subject == todo.Subject && t.Done == todo.Done);
        }
    }

    [Fact]
    public async Task Duplicate_CreatesDuplicatedTodoAndReturnsCreated()
    {
        // Arrange
        var todo = SeedTodos(1)[0];
        // Act
        var response = await _client.PostAsync($"/api/Todo/{todo.Id}/duplicate", null);
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var duplicated = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(duplicated);
        Assert.NotEqual(todo.Id, duplicated!.Id);
        Assert.Equal(todo.Subject, duplicated.Subject);
        Assert.Equal(todo.Done, duplicated.Done);
        Assert.True(duplicated.CreatedAt > todo.CreatedAt || duplicated.CreatedAt == duplicated.UpdatedAt);
        Assert.True(duplicated.CreatedAt > DateTime.MinValue);
        Assert.True(duplicated.UpdatedAt > DateTime.MinValue);
        Assert.Equal(duplicated.CreatedAt, duplicated.UpdatedAt);
    }
}
