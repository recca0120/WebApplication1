using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Extensions
{
    public static class QueryableExtensions
    {
        public static PagedResult<T> Paginate<T>(this IQueryable<T> query, int page = 1, int pageSize = 15)
        {
            return PagedResult<T>.FromQuery(query, page, pageSize);
        }
    }
}
