using CO.CDP.RegisterOfCommercialTools.App.Validation;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Validation;

public class FeesValidatorAttributeTest
{
    [Fact]
    public void IsValid_WhenNoFeesIsSelectedAndFeeValuesAreProvided_ReturnsValidationError()
    {
        var model = new TestModel { FeeFrom = 10, FeeTo = 20, NoFees = "true" };
        var validationContext = new ValidationContext(model);
        var attribute = new FeesValidatorAttribute(nameof(model.FeeFrom), nameof(model.FeeTo), nameof(model.NoFees));

        var result = attribute.GetValidationResult(model, validationContext);

        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Fee values cannot be provided when 'No fees' is selected.");
    }

    [Fact]
    public void IsValid_WhenNoFeesIsSelectedAndFeeValuesAreNull_ReturnsSuccess()
    {
        var model = new TestModel { FeeFrom = null, FeeTo = null, NoFees = "true" };
        var validationContext = new ValidationContext(model);
        var attribute = new FeesValidatorAttribute(nameof(model.FeeFrom), nameof(model.FeeTo), nameof(model.NoFees));

        var result = attribute.GetValidationResult(model, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenNoFeesIsNotSelectedAndFeeValuesAreProvided_ReturnsSuccess()
    {
        var model = new TestModel { FeeFrom = 10, FeeTo = 20, NoFees = null };
        var validationContext = new ValidationContext(model);
        var attribute = new FeesValidatorAttribute(nameof(model.FeeFrom), nameof(model.FeeTo), nameof(model.NoFees));

        var result = attribute.GetValidationResult(model, validationContext);

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenInvalidPropertyNamesAreProvided_ReturnsValidationError()
    {
        var model = new TestModel { FeeFrom = 10, FeeTo = 20, NoFees = "true" };
        var validationContext = new ValidationContext(model);
        var attribute = new FeesValidatorAttribute("InvalidFrom", "InvalidTo", "InvalidNoFees");

        var result = attribute.GetValidationResult(model, validationContext);

        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Invalid property names provided to the FeesValidatorAttribute.");
    }

    private class TestModel
    {
        public decimal? FeeFrom { get; set; }
        public decimal? FeeTo { get; set; }
        public string? NoFees { get; set; }
    }
}

