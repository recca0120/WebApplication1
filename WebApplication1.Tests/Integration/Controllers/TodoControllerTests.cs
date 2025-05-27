using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Controllers;
using WebApplication1.Models;
using Xunit;
using Bogus;

namespace WebApplication1.Tests.Integration.Controllers;

public class TodoControllerTests : IClassFixture<TodoTestFixture>
{
    private readonly TodoTestFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TodoControllerTests(TodoTestFixture fixture)
    {
        _fixture = fixture;
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    private static readonly Faker<Todo> _todoFaker = new Faker<Todo>()
        .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
        .RuleFor(t => t.Done, f => f.Random.Bool());

    private List<Todo> SeedTodos(int count, bool doneRandom = true)
    {
        var todos = doneRandom
            ? _todoFaker.Generate(count)
            : new Faker<Todo>()
                .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
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
        var newTodo = new Todo { Subject = "Test Subject", Done = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Todo", newTodo);
        var created = await response.Content.ReadFromJsonAsync<Todo>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(newTodo.Subject, created!.Subject);
        Assert.Equal(newTodo.Done, created.Done);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Update_ChangesTodoAndReturnsOk()
    {
        // Arrange
        var todo = SeedTodos(1, false)[0];
        // Act
        var update = new Todo { Subject = todo.Subject, Done = true };
        var response = await _client.PutAsJsonAsync($"/api/Todo/{todo.Id}", update);
        var updated = await response.Content.ReadFromJsonAsync<Todo>();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(update.Subject, updated!.Subject);
        Assert.True(updated.Done);
        Assert.Equal(todo.Id, updated.Id);
    }

    [Fact]
    public async Task Delete_RemovesTodoAndReturnsNoContent()
    {
        var todo = SeedTodos(1)[0];
        var response = await _client.DeleteAsync($"/api/Todo/{todo.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(_fixture.Exists(todo));
    }

    [Fact]
    public async Task GetAll_ReturnsPagedTodos()
    {
        _fixture.ResetDb();
        var todos = SeedTodos(15);
        var response = await _client.GetAsync("/api/Todo?page=2&pageSize=5");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Todo>>();
        Assert.NotNull(result);
        Assert.Equal(15, result!.total);
        Assert.Equal(5, result.items.Count);
    }

    [Fact]
    public async Task GetAll_ReturnsAllTodos()
    {
        _fixture.ResetDb();
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
    public async Task Duplicated_CreatesDuplicatedTodoAndReturnsCreated()
    {
        // Arrange
        _fixture.ResetDb();
        var todo = SeedTodos(1)[0];
        // Act
        var response = await _client.PostAsync($"/api/Todo/{todo.Id}/duplicated", null);
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var duplicated = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(duplicated);
        Assert.NotEqual(todo.Id, duplicated!.Id);
        Assert.Equal(todo.Subject, duplicated.Subject);
        Assert.Equal(todo.Done, duplicated.Done);
        Assert.True(duplicated.CreatedAt > todo.CreatedAt || duplicated.CreatedAt == duplicated.UpdatedAt);
    }
}
