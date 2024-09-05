using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation.Tests;

public class ValidUriAttributeTests
{
    private readonly ValidUriAttribute _validUriAttribute = new();

    [Theory]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com", true)]
    [InlineData("ftp://example.com", true)]
    [InlineData("//example.com", true)]
    [InlineData("invalid-uri", false)]
    [InlineData("https://.example.com", false)]
    [InlineData("", true)]
    [InlineData(null, true)]
    public void IsValid_ShouldValidateUri_Correctly(string? input, bool expectedIsValid)
    {
        var validationContext = new ValidationContext(new object(), null, null);

        var result = _validUriAttribute.GetValidationResult(input, validationContext);

        if (expectedIsValid)
        {
            result.Should().Be(ValidationResult.Success);
        }
        else
        {
            result.Should().NotBe(ValidationResult.Success);
            result.As<ValidationResult>().ErrorMessage.Should().Be($"{validationContext.DisplayName} is invalid");
        }
    }

    [Fact]
    public void IsValid_ShouldReturnCustomErrorMessage_WhenInvalidUri()
    {
        var input = "invalid-uri";
        var validationContext = new ValidationContext(new object(), null, null)
        {
            DisplayName = "Test Uri"
        };
        _validUriAttribute.ErrorMessage = "Custom error message";

        var result = _validUriAttribute.GetValidationResult(input, validationContext);

        result.Should().NotBe(ValidationResult.Success);
        result.As<ValidationResult>().ErrorMessage.Should().Be("Custom error message");
    }
}