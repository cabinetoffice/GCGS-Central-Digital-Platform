using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for generating URL-friendly slugs.
/// </summary>
public partial class SlugGeneratorService : ISlugGeneratorService
{
    public string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);

        // Replace spaces with hyphens
        slug = SpacesRegex().Replace(slug, "-");

        // Replace invalid characters with hyphens
        slug = InvalidCharsRegex().Replace(slug, "-");

        // Replace multiple hyphens with single hyphen
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    [GeneratedRegex(@"[^a-z0-9-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
