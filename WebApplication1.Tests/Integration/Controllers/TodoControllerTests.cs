using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Models;
using Xunit;
using Bogus;

namespace WebApplication1.Tests.Integration.Controllers;

public class TodoControllerTests : IClassFixture<TodoTestFixture>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TodoControllerTests(TodoTestFixture fixture)
    {
        _factory = fixture.Factory;
    }

    [Fact]
    public async Task Create_AddsTodoAndReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var faker = new Faker<Todo>()
            .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
            .RuleFor(t => t.Done, f => f.Random.Bool());
        var newTodo = faker.Generate();

        // Act
        var response = await client.PostAsJsonAsync("/api/Todo", newTodo);
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
        var client = _factory.CreateClient();
        var faker = new Faker<Todo>()
            .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
            .RuleFor(t => t.Done, f => false);
        var newTodo = faker.Generate();
        var createResponse = await client.PostAsJsonAsync("/api/Todo", newTodo);
        var created = await createResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(created);
        Assert.False(created!.Done);

        // Act
        var update = new Todo { Subject = created.Subject, Done = true };
        var updateResponse = await client.PutAsJsonAsync($"/api/Todo/{created.Id}", update);
        var updated = await updateResponse.Content.ReadFromJsonAsync<Todo>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(update.Subject, updated!.Subject);
        Assert.True(updated.Done);
        Assert.Equal(created.Id, updated.Id);
    }

    [Fact]
    public async Task Delete_RemovesTodoAndReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var faker = new Faker<Todo>()
            .RuleFor(t => t.Subject, f => f.Lorem.Sentence(3))
            .RuleFor(t => t.Done, f => f.Random.Bool());
        var newTodo = faker.Generate();
        var createResponse = await client.PostAsJsonAsync("/api/Todo", newTodo);
        var created = await createResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(created);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/Todo/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 確認已刪除
        var getResponse = await client.GetAsync($"/api/Todo/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
