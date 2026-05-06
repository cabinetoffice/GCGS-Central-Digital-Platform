using System.Text.RegularExpressions;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;

public static partial class SlugGenerator
{
    public static string Generate(string name)
    {
        var slug = name.ToLowerInvariant().Trim();
        slug = InvalidCharsRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = MultiHyphenRegex().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex("[^a-z0-9\\s-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex("[\\s]")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiHyphenRegex();
}
