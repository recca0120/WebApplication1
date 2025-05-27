using System.Linq;
using WebApplication1.Models;
using WebApplication1.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly TodoDbContext _db;
    public TodoController(TodoDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        var query = _db.Todos.OrderByDescending(t => t.Id);
        return Ok(query.Paginate(page, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Todo todo)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        todo.Done = todo.Done == true;
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Todo todo)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var entity = await _db.Todos.FindAsync(id);
        if (entity == null)
            return NotFound();
        entity.Subject = todo.Subject;
        entity.Description = todo.Description;
        entity.Done = todo.Done;
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

    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(int id)
    {
        var entity = await _db.Todos.FindAsync(id);
        if (entity == null)
            return NotFound();
        var duplicated = new Todo
        {
            Subject = entity.Subject,
            Done = entity.Done
            // CreatedAt/UpdatedAt 由 DbContext 自動處理
        };
        _db.Todos.Add(duplicated);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = duplicated.Id }, duplicated);
    }
}
