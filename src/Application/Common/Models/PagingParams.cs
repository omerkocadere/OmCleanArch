namespace CleanArch.Application.Common.Models;

public class PagingParams
{
    private const int MaxPageSize = 50;

    // Nullable for binding, computed properties for business logic
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    // Computed properties with defaults
    public int PageNumberValue => PageNumber ?? 1;
    public int PageSizeValue
    {
        get
        {
            if (!PageSize.HasValue)
                return 10;

            return PageSize.Value > MaxPageSize ? MaxPageSize : PageSize.Value;
        }
    }
}
