using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1;
using WebApplication1.Models;

namespace WebApplication1.Tests.Integration.Controllers;

public class TodoTestFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }

    public TodoTestFixture()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddDbContext<TodoDbContext>(options =>
                    options.UseInMemoryDatabase("TodoTestDb").EnableSensitiveDataLogging());

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    public void ResetDb()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Todos.RemoveRange(db.Todos);
        db.SaveChanges();
    }

    public bool Exists(Todo todo)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        return db.Todos.Any(t => t.Id == todo.Id && t.Subject == todo.Subject && t.Done == todo.Done);
    }

    public void Dispose()
    {
        // 可選：釋放資源
    }
}
