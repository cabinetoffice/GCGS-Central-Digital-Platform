using CO.CDP.ApplicationRegistry.Infrastructure.Services;
using FluentAssertions;

namespace CO.CDP.ApplicationRegistry.UnitTests.Services;

public class SlugGeneratorServiceTests
{
    private readonly SlugGeneratorService _service;

    public SlugGeneratorServiceTests()
    {
        _service = new SlugGeneratorService();
    }

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Test Organisation", "test-organisation")]
    [InlineData("Company Inc.", "company-inc")]
    [InlineData("My-Company-Name", "my-company-name")]
    [InlineData("Special & Characters!", "special-characters")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    [InlineData("  Leading And Trailing  ", "leading-and-trailing")]
    public void GenerateSlug_WithVariousInputs_ReturnsCorrectSlug(string input, string expected)
    {
        // Act
        var result = _service.GenerateSlug(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateSlug_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = _service.GenerateSlug("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSlug_WithWhitespace_ReturnsEmptyString()
    {
        // Act
        var result = _service.GenerateSlug("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("MixedCase", "mixedcase")]
    public void GenerateSlug_WithVariousCases_ReturnsLowercaseSlug(string input, string expected)
    {
        // Act
        var result = _service.GenerateSlug(input);

        // Assert
        result.Should().Be(expected);
    }
}
