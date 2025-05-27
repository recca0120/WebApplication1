namespace WebApplication1.Models;

public class Todo
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool Done { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
