using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Validation;

namespace CO.CDP.OrganisationApp.Tests.Validation;

public class NumberAttributeTests
{
    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsEmptyString_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsWhitespace_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("   ", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidPositiveInteger_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("123", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidNegativeInteger_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("-123", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidPositiveDecimal_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("123.45", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsValidNegativeDecimal_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("-123.45", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsZero_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("0", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsZeroDecimal_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("0.0", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsVeryLargeNumber_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("999999999999999999.99", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNotANumber_ReturnsError()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("abc", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Number_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueContainsLetters_ReturnsError()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("123abc", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Number_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsSpecialCharacters_ReturnsError()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("!@#$", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Number_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueHasMultipleDecimalPoints_ReturnsError()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("123.45.67", validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_Number_InvalidError, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsEmptyWithSpaces_ReturnsSuccess()
    {
        var attribute = new NumberAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult("  \t  ", validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}