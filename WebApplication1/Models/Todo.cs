namespace WebApplication1.Models;

public class Todo
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    [System.ComponentModel.DefaultValue(false)]
    public bool Done { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
