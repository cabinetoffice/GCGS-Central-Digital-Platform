namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Represents context parameters that should be preserved across navigation between services
/// </summary>
public class ContextParams
{
    /// <summary>
    /// The language/culture code (e.g., "en_GB")
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// The origin location (e.g., "buyer-view")
    /// </summary>
    public string? Origin { get; init; }

    /// <summary>
    /// The organisation identifier
    /// </summary>
    public Guid? OrganisationId { get; init; }

    /// <summary>
    /// The cookie acceptance status
    /// </summary>
    public string? CookiesAccepted { get; init; }

    /// <summary>
    /// Converts the context parameters to a dictionary, excluding null or empty values
    /// </summary>
    /// <returns>Dictionary of non-null context parameters</returns>
    public Dictionary<string, string?> ToDictionary() =>
        new Dictionary<string, string?>
        {
            ["language"] = Language,
            ["origin"] = Origin,
            ["organisation_id"] = OrganisationId?.ToString(),
            ["cookies_accepted"] = CookiesAccepted
        }
        .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
