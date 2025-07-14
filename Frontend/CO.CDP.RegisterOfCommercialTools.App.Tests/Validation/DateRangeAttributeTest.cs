using CO.CDP.RegisterOfCommercialTools.App.Validation;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Validation;

public class DateRangeAttributeTest
{
    private class TestModel
    {
        public DateOnly? FromDate { get; init; }

        [DateRange("FromDate", ErrorMessage = "To date must be after from date")]
        public DateOnly? ToDate { get; init; }
    }

    [Fact]
    public void IsValid_WhenToDateIsLaterThanFromDate_ShouldBeValid()
    {
        var model = new TestModel
        {
            FromDate = new DateOnly(2025, 1, 1),
            ToDate = new DateOnly(2025, 1, 2)
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenToDateIsSameAsFromDate_ShouldBeValid()
    {
        var model = new TestModel
        {
            FromDate = new DateOnly(2025, 1, 1),
            ToDate = new DateOnly(2025, 1, 1)
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenToDateIsEarlierThanFromDate_ShouldBeInvalid()
    {
        var model = new TestModel
        {
            FromDate = new DateOnly(2025, 1, 2),
            ToDate = new DateOnly(2025, 1, 1)
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("To date must be after from date");
    }

    [Fact]
    public void IsValid_WhenFromDateIsNull_ShouldBeValid()
    {
        var model = new TestModel
        {
            FromDate = null,
            ToDate = new DateOnly(2025, 1, 1)
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenToDateIsNull_ShouldBeValid()
    {
        var model = new TestModel
        {
            FromDate = new DateOnly(2025, 1, 1),
            ToDate = null
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenBothDatesAreNull_ShouldBeValid()
    {
        var model = new TestModel
        {
            FromDate = null,
            ToDate = null
        };
        var validationContext = new ValidationContext(model);
        var attribute = new DateRangeAttribute("FromDate") { ErrorMessage = "To date must be after from date" };

        var result = attribute.GetValidationResult(model.ToDate, validationContext);

        result.Should().Be(ValidationResult.Success);
    }
}
