using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Validation;

namespace CO.CDP.OrganisationApp.Tests.Validation;

public class DateRangeAttributeTests
{
    [Fact]
    public void IsValid_WhenDateIsWithinRange_ReturnsSuccess()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-5);
        var maxDate = DateTimeOffset.UtcNow.AddDays(5);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDateIsBeforeRange_ReturnsError()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-1);
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(-5), validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(string.Format(StaticTextResource.Global_DateInput_DateMustBeBetween, minDate.Date.ToShortDateString(), maxDate.Date.ToShortDateString()), result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenDateIsAfterRange_ReturnsError()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-1);
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(DateTimeOffset.UtcNow.AddDays(5), validationContext);

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal(string.Format(StaticTextResource.Global_DateInput_DateMustBeBetween, minDate.Date.ToShortDateString(), maxDate.Date.ToShortDateString()), result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenDateIsEqualToMinDate_ReturnsSuccess()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-1);
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(minDate, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenDateIsEqualToMaxDate_ReturnsSuccess()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-1);
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(maxDate, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-1);
        var maxDate = DateTimeOffset.UtcNow.AddDays(1);
        var attribute = new DateRangeAttribute(minDate.ToString("o"), maxDate.ToString("o"));
        var validationContext = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}
