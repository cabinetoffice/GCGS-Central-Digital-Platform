using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Extensions;

public static class StringExtensions
{
    public static string StripAndRemoveObscureWhitespace(this string value)
    {
        value = value.Trim();
        value = Regex.Replace(value, @"[\u200B-\u200D\uFEFF]", string.Empty);

        return value;
    }
}
