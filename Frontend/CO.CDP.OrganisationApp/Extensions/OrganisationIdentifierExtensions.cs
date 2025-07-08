namespace CO.CDP.OrganisationApp.Extensions;

/// <summary>
/// Extension methods for working with organisation identifiers.
/// </summary>
public static class OrganisationIdentifierExtensions
{
    /// <summary>
    /// Checks if the organisation has a GB-PPON identifier either as primary identifier or in additional identifiers
    /// and returns the PPON value if found
    /// </summary>
    /// <param name="organisation">The organisation to check</param>
    /// <returns>A tuple with a boolean indicating if a GB-PPON identifier was found and the PPON value if available</returns>
    public static (bool HasPpon, string? PponValue) GetGbPponIdentifier(this Organisation.WebApiClient.Organisation organisation)
    {
        if (organisation.Identifier?.Scheme == "GB-PPON")
        {
            return (true, FormatPponForDisplay(organisation.Identifier.Id));
        }

        if (organisation.AdditionalIdentifiers == null)
        {
            return (false, null);
        }

        var pponIdentifier = organisation.AdditionalIdentifiers.FirstOrDefault(id => id.Scheme == "GB-PPON");
        return pponIdentifier != null
            ? (true, FormatPponForDisplay(pponIdentifier.Id))
            : (false, null);
    }

    /// <summary>
    /// Formats a PPON identifier for display by removing the "GB-PPON:" prefix if present
    /// </summary>
    /// <param name="pponId">The raw PPON identifier</param>
    /// <returns>The formatted PPON identifier</returns>
    public static string FormatPponForDisplay(string pponId)
    {
        if (string.IsNullOrEmpty(pponId))
        {
            return string.Empty;
        }

        return pponId.StartsWith("GB-PPON:", StringComparison.OrdinalIgnoreCase)
            ? pponId.Substring("GB-PPON:".Length)
            : pponId;
    }

    /// <summary>
    /// Gets the PPON value from an organisation, checking both primary and additional identifiers
    /// </summary>
    /// <param name="organisation">The organisation to get the PPON from</param>
    /// <returns>The formatted PPON value or null if not found</returns>
    public static string? GetPponValue(this Organisation.WebApiClient.Organisation organisation)
    {
        var (hasPpon, pponValue) = GetGbPponIdentifier(organisation);
        return hasPpon ? pponValue : null;
    }

    /// <summary>
    /// Determines if a string is likely a PPON identifier and formats it appropriately
    /// </summary>
    /// <param name="query">The string to check</param>
    /// <returns>A tuple with a boolean indicating if the string is likely a PPON and the formatted PPON value if applicable</returns>
    public static (bool IsLikelyPpon, string? FormattedPpon) IsLikelyPpon(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (false, null);
        }

        if (query.StartsWith("GB-PPON:", StringComparison.OrdinalIgnoreCase))
        {
            return (true, query);
        }

        if (query.StartsWith("GB-PPON-", StringComparison.OrdinalIgnoreCase))
        {
            return (true, "GB-PPON:" + query.Substring("GB-PPON-".Length));
        }

        var pponRegex = new System.Text.RegularExpressions.Regex("^[A-Z]{4}-\\d{4}-[A-Z0-9]{4}$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (pponRegex.IsMatch(query))
        {
            return (true, "GB-PPON:" + query);
        }

        return (false, null);
    }
}
