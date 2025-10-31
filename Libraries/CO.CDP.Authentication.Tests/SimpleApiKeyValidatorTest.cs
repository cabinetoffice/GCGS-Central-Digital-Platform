using FluentAssertions;

namespace CO.CDP.Authentication.Tests;

public class SimpleApiKeyValidatorTest
{
    private readonly SimpleApiKeyValidator _validator = new();

    [Fact]
    public async Task Validate_WithMatchingApiKeys_ReturnsTrue()
    {
        var apiKey = "test-api-key";
        var configuredApiKey = "test-api-key";

        var result = await _validator.Validate(apiKey, configuredApiKey);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonMatchingApiKeys_ReturnsFalse()
    {
        var apiKey = "test-api-key";
        var configuredApiKey = "different-api-key";

        var result = await _validator.Validate(apiKey, configuredApiKey);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "configured-key")]
    [InlineData("", "configured-key")]
    [InlineData(" ", "configured-key")]
    [InlineData("api-key", null)]
    [InlineData("api-key", "")]
    [InlineData("api-key", " ")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public async Task Validate_WithNullOrWhitespaceKeys_ReturnsFalse(string? apiKey, string? configuredApiKey)
    {
        var result = await _validator.Validate(apiKey, configuredApiKey);

        result.Should().BeFalse();
    }
}
