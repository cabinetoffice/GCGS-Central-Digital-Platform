using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Validation;

namespace CO.CDP.OrganisationApp.Tests.Validation;

public class PercentageAttributeTests
{
    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsEmptyString_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsWhitespace_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("   ", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsZero_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("0", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsOneHundred_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("100", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidPercentageInteger_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("50", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidPercentageDecimal_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("75.5", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidPercentageWithManyDecimals_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("33.333333", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsZeroDecimal_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("0.0", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsOneHundredDecimal_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("100.0", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNegative_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("-1", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsOverOneHundred_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("101", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsOverOneHundredDecimal_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("100.1", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsNotANumber_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("abc", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueContainsLetters_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("50%", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsSpecialCharacters_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("!@#$", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueHasMultipleDecimalPoints_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("12.34.56", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsVeryLargeNumber_ReturnsError()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("999999", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Percentage_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsEmptyWithSpaces_ReturnsSuccess()
    {
        var attribute = new PercentageAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("  \t  ", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}