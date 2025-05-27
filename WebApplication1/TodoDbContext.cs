using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
