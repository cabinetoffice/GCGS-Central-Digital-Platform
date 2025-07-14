using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Validation;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Validation;

public class DecimalRangeAttributeTest
{
    private class TestModel
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal? NullableValue { get; set; }
        public string StringProperty { get; set; } = string.Empty;
    }

    [Fact]
    public void IsValid_WhenMaxValueIsGreaterThanMinValue_ReturnsSuccess()
    {
        var model = new TestModel { MinValue = 10.0m, MaxValue = 20.0m };
        var attribute = new DecimalRangeAttribute("MinValue") { ErrorMessage = "Max value must be greater than or equal to Min value" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.MaxValue) };

        var result = attribute.GetValidationResult(model.MaxValue, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenMaxValueIsEqualToMinValue_ReturnsSuccess()
    {
        var model = new TestModel { MinValue = 10.0m, MaxValue = 10.0m };
        var attribute = new DecimalRangeAttribute("MinValue") { ErrorMessage = "Max value must be greater than or equal to Min value" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.MaxValue) };

        var result = attribute.GetValidationResult(model.MaxValue, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenMaxValueIsLessThanMinValue_ReturnsValidationError()
    {
        var model = new TestModel { MinValue = 20.0m, MaxValue = 10.0m };
        var errorMessage = "Max value must be greater than or equal to Min value";
        var attribute = new DecimalRangeAttribute("MinValue") { ErrorMessage = errorMessage };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.MaxValue) };

        var result = attribute.GetValidationResult(model.MaxValue, validationContext);

        Assert.NotNull(result);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenCompareToPropertyDoesNotExist_ReturnsValidationError()
    {
        var model = new TestModel { MaxValue = 10.0m };
        var attribute = new DecimalRangeAttribute("NonExistentProperty");
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.MaxValue) };

        var result = attribute.GetValidationResult(model.MaxValue, validationContext);

        Assert.NotNull(result);
        Assert.Equal("Unknown property: NonExistentProperty", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_WhenValueIsNotDecimal_ReturnsSuccess()
    {
        var model = new TestModel { MinValue = 10.0m, StringProperty = "Not a decimal" };
        var attribute = new DecimalRangeAttribute("MinValue");
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.StringProperty) };

        var result = attribute.GetValidationResult(model.StringProperty, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenCompareToValueIsNotDecimal_ReturnsSuccess()
    {
        var model = new TestModel { MaxValue = 10.0m, StringProperty = "Not a decimal" };
        var attribute = new DecimalRangeAttribute("StringProperty");
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.MaxValue) };

        var result = attribute.GetValidationResult(model.MaxValue, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        var model = new TestModel { MinValue = 10.0m, NullableValue = null };
        var attribute = new DecimalRangeAttribute("MinValue");
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.NullableValue) };

        var result = attribute.GetValidationResult(model.NullableValue, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }
}
