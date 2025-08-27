using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Defines filtering options for organisation search results.
/// Can be combined using bitwise operations for multiple filters.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganisationSearchFilter
{
    /// <summary>
    /// Exclude organisations that have only pending buyer roles (no other active roles).
    /// Used by PPON search to hide organisations that are not yet fully active.
    /// </summary>
    ExcludeOnlyPendingBuyerRoles = 1,
}