namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for generating URL-friendly slugs.
/// </summary>
public interface ISlugGeneratorService
{
    /// <summary>
    /// Generates a URL-friendly slug from the given text.
    /// </summary>
    /// <param name="text">The text to convert to a slug.</param>
    /// <returns>A URL-friendly slug.</returns>
    string GenerateSlug(string text);
}
