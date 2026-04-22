namespace ImovelStand.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int total)
        => new() { Items = items, Page = page, PageSize = pageSize, Total = total };
}

public class PageRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public (int page, int pageSize) Normalized(int maxPageSize = 100)
    {
        var p = Page < 1 ? 1 : Page;
        var s = PageSize < 1 ? 20 : PageSize > maxPageSize ? maxPageSize : PageSize;
        return (p, s);
    }
}
