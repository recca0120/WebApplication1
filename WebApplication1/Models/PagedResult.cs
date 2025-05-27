namespace WebApplication1.Models
{
    public class PagedResult<T>
    {
        public int total { get; set; }
        public List<T> items { get; set; } = new();
    }
}