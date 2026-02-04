namespace SEMS.Core.Common;

public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } // "asc" or "desc"
    public string? SearchTerm { get; set; }
}
