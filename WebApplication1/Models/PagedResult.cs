namespace WebApplication1.Models
{
    public class PagedResult<T>
    {
        public int total { get; set; }
        public int pageSize { get; set; }
        public int current_page { get; set; }
        public List<T> items { get; set; } = new();

        public int from => total == 0 ? 0 : (current_page - 1) * pageSize + 1;
        public int to => from + items.Count - 1;
        public int total_page => (int)Math.Ceiling(total / (double)pageSize);
        public int last_page => total_page;

        public static PagedResult<T> FromQuery(IQueryable<T> query, int page = 1, int pageSize = 15)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 15 : pageSize;
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>
            {
                total = total,
                pageSize = pageSize,
                current_page = page,
                items = items
            };
        }
    }
}