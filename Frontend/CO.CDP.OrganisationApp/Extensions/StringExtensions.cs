namespace CO.CDP.OrganisationApp.Extensions;

public static class StringExtensions
{
    private static readonly string[] obscureWhitespaces =
    [
        "\u180E",       // Mongolian vowel separator
        "\u200B",       // zero width space
        "\u200C",       // zero width non-joiner
        "\u200D",       // zero width joiner
        "\u2060",       // word joiner
        "\uFEFF",       // zero width non-breaking space
        "\u2028",       // line separator
        "\u2029",       // paragraph separator
        "\u00A0\u202F"  // non breaking space + narrow no break space
    ];

    public static string StripAndRemoveObscureWhitespaces(this string value)
    {
        if (value == string.Empty)
        {
            return value;
        }

        value = value.Trim();
        foreach (string c in obscureWhitespaces)
        {
            value = value.Replace(c, string.Empty);
        }

        return value;
    }
}