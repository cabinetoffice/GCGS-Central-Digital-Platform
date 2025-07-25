using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Validation;

namespace CO.CDP.OrganisationApp.Tests.Validation;

public class MaxDateAttributeTests
{
    [Fact]
    public void IsValid_WhenDateIsBeforeMaxDate_ReturnsSuccess()
    {
        var maxDate = DateTimeOffset.UtcNow.AddDays(5);
        var attribute = new MaxDateAttribute(maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(1), validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDateIsAfterMaxDate_ReturnsError()
    {
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new MaxDateAttribute(maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(5), validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(string.Format(StaticTextResource.Global_DateInput_DateMustBeOnOrBefore, maxDate.Date.ToShortDateString()), result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenDateIsEqualToMaxDate_ReturnsSuccess()
    {
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new MaxDateAttribute(maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(maxDate, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new MaxDateAttribute(maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}
