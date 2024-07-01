using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.Authentication.Tests;

public class ApiKeyValidatorTest
{
    private readonly ApiKeyValidator _validator;
    private const string ValidApiKey = "valid-api-key";
    private const string InvalidApiKey = "invalid-api-key";

    public ApiKeyValidatorTest()
    {
        IConfiguration configRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("CdpApiKeys:0", ValidApiKey)
            ])
            .Build();

        _validator = new ApiKeyValidator(configRoot);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_NullOrWhitespaceApiKey_ThrowsArgumentException(string? apiKey)
    {
        Func<Task> act = async () => await _validator.Validate(apiKey);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Validate_ValidApiKey_ReturnsTrue()
    {
        var result = await _validator.Validate(ValidApiKey);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_InvalidApiKey_ReturnsFalse()
    {
        var result = await _validator.Validate(InvalidApiKey);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_MissingConfiguration_ThrowsException()
    {
        IConfiguration configRoot = new ConfigurationBuilder().Build();

        Func<Task> act = async () => await new ApiKeyValidator(configRoot).Validate("any-api-key");

        await act.Should().ThrowAsync<Exception>()
                 .WithMessage("Missing configuration key: CdpApiKeys");
    }
}
