using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;

public class HelperTests
{
    [Theory]
    [InlineData("/relative/path", true)]        // Valid relative URI
    [InlineData("relative/path", true)]         // Valid relative URI without a leading slash
    [InlineData("", false)]                    // Empty string
    [InlineData(null, false)]                  // Null value
    [InlineData(" ", false)]                   // Whitespace
    [InlineData("http://example.com", false)]  // Absolute URI (not relative)
    [InlineData("/path/with spaces", true)]    // Relative URI with spaces

    public void ValidRelativeUri_ShouldReturnExpectedResults(string? input, bool expected)
    {
        var result = Helper.ValidRelativeUri(input);

        result.Should().Be(expected);
    }
}