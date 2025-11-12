using System.Text.RegularExpressions;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;

/// <summary>
/// Provides methods to sanitize strings for safe logging, preventing log injection attacks.
/// </summary>
public static partial class LogSanitizer
{
    [GeneratedRegex(@"[\r\n\t\x00-\x1F\x7F]")]
    private static partial Regex ControlCharactersRegex();

    /// <summary>
    /// Sanitizes input string by removing control characters that could be used for log injection.
    /// Removes newlines, carriage returns, tabs, and other control characters (0x00-0x1F, 0x7F).
    /// </summary>
    /// <param name="input">The string to sanitize</param>
    /// <returns>Sanitized string safe for logging</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return ControlCharactersRegex().Replace(input, string.Empty);
    }

    /// <summary>
    /// Sanitizes a list of strings by removing control characters from each item and joining them with a separator.
    /// </summary>
    /// <param name="items">The list of strings to sanitize</param>
    /// <param name="separator">The separator to use when joining (defaults to ", ")</param>
    /// <returns>Sanitized comma-separated string safe for logging</returns>
    public static string SanitizeList(List<string>? items, string separator = ", ")
    {
        if (items == null || items.Count == 0)
            return string.Empty;

        return string.Join(separator, items.Select(Sanitize));
    }
}