namespace CleanArch.Application.Common.Models;

// Base query parameters record that combines paging and sorting functionality.
// Derived records should add their specific filter properties for automatic model binding.
public record QueryParamsBase
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

    /// <summary>
    /// OrderBy string in System.Linq.Dynamic.Core format
    /// Examples: "Name", "Name desc", "Age, Name desc"
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>
    /// Search term for filtering across relevant text fields
    /// </summary>
    public string? SearchTerm { get; init; }
}
