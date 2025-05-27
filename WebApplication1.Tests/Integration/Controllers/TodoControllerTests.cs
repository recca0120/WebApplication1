using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Models;
using Xunit;

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
        var newTodo = new Todo { Subject = "test", Done = false };

        // Act
        var response = await client.PostAsJsonAsync("/api/Todo", newTodo);
        var created = await response.Content.ReadFromJsonAsync<Todo>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("test", created!.Subject);
        Assert.False(created.Done);
        Assert.True(created.Id > 0);
    }
}
