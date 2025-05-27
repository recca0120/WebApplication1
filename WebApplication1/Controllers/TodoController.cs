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
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        var query = _db.Todos.OrderByDescending(t => t.Id);
        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResult<Todo> { total = total, items = items });
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Todo todo)
    {
        var entity = await _db.Todos.FindAsync(id);
        if (entity == null)
            return NotFound();
        entity.Subject = todo.Subject;
        entity.Done = todo.Done;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Todos.FindAsync(id);
        if (entity == null)
            return NotFound();
        _db.Todos.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var entity = await _db.Todos.FindAsync(id);
        if (entity == null)
            return NotFound();
        return Ok(entity);
    }

    private class PagedResult<T>
    {
        public int total { get; set; }
        public List<T> items { get; set; } = new();
    }
}
