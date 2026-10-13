using System.Text.Json.Serialization;

namespace CalibrationManagement.Application.Common;

/// <summary>Base class for rows returned by paged stored procedures (COUNT(*) OVER () AS TotalCount).</summary>
public abstract class PagedRow
{
    [JsonIgnore]
    public int TotalCount { get; set; }
}

public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize < 1 ? 25 : (PageSize > 200 ? 200 : PageSize);
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
