namespace CO.CDP.UI.Foundation.Utilities;

public class InputSanitiser
{
    /// <summary>
    /// Sanitises a text input by trimming whitespace, normalising line endings, and removing control characters.
    /// </summary>
    /// <param name="input">The input string to sanitise.</param>
    /// <returns>A sanitised string, or null if input is null.</returns>
    public static string? SanitiseSingleLineTextInput(string? input)
    {
        if (input == null)
            return null;

        var noAngleBrackets = input.Replace("<", string.Empty).Replace(">", string.Empty);

        var noControlChars = System.Text.RegularExpressions.Regex.Replace(noAngleBrackets, "[\x00-\x1F\x7F]", " ");

        var normalisedWhitespace = System.Text.RegularExpressions.Regex.Replace(noControlChars, "\\s+", " ").Trim();

        return normalisedWhitespace;
    }
}