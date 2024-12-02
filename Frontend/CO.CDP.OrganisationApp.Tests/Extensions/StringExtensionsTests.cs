using CO.CDP.OrganisationApp.Extensions;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("validated-text")]
    [InlineData("  \tvalidated-text \x0c ")]
    [InlineData("\rv\u200Cal\u200Did\u200Bate\u2060d-\u180Etex\uFEFFt\uFEFF")]
    public void StripAndRemoveObscureWhitespaces_ShouldSucceed(string value)
    {
        var result = StringExtensions.StripAndRemoveObscureWhitespaces(value);

        result.Should().Be("validated-text");
    }
}