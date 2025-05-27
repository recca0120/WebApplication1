using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly TodoDbContext _db;
    public TodoController(TodoDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_db.Todos.ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Todo todo)
    {
        todo.CreatedAt = DateTime.UtcNow;
        todo.UpdatedAt = DateTime.UtcNow;
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = todo.Id }, todo);
    }
}
