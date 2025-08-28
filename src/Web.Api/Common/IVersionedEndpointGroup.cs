namespace CleanArch.Web.Api.Common;

/// <summary>
/// Marker interface for endpoint groups that need API versioning support.
/// Implement this interface on endpoint groups that require versioned routes.
/// </summary>
public interface IVersionedEndpointGroup
{
    /// <summary>
    /// Gets the supported API versions for this endpoint group.
    /// Default implementation returns versions 1 and 2.
    /// </summary>
    IEnumerable<int> SupportedVersions => [1, 2];
}
