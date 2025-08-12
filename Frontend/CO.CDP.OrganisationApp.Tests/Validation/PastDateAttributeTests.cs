using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Validation;

namespace CO.CDP.OrganisationApp.Tests.Validation;

public class PastDateAttributeTests
{
    [Fact]
    public void IsValid_WhenDateIsPast_ReturnsSuccess()
    {
        var attribute = new PastDateAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(-1), validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDateIsFuture_ReturnsError()
    {
        var attribute = new PastDateAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(1), validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(StaticTextResource.Global_DateInput_DateInPastValidation, result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenDateIsPresent_ReturnsError()
    {
        var attribute = new PastDateAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow, validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var attribute = new PastDateAttribute();
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}
