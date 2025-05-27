namespace WebApplication1.Models
{
    public class PagedResult<T>
    {
        public int total { get; set; }
        public List<T> items { get; set; } = new();
        public int from { get; set; }
        public int to { get; set; }
        public int current_page { get; set; }
        public int total_page { get; set; }
        public int last_page { get; set; }
    }
}