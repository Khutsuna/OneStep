namespace OneStep.Application.Common;

/// <summary>
/// One page of a longer list, plus what the client needs to draw page buttons.
/// Every list endpoint returns this same shape.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
