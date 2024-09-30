using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementUrlInputModelTests
{
    [Fact]
    public void GetAnswer_ShouldReturnNull_WhenIsNotRequiredAndHasNoValue()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = false,
            HasValue = false
        };

        var result = model.GetAnswer();

        result.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_ShouldReturnAnswer_WhenTextInputIsProvided()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = true,
            TextInput = "https://example.com",
            HasValue = true
        };

        var result = model.GetAnswer();

        result.Should().NotBeNull();
        result?.TextValue.Should().Be("https://example.com");
    }

    [Fact]
    public void SetAnswer_ShouldSetTextInputAndHasValue_WhenAnswerIsProvided()
    {
        var model = new FormElementUrlInputModel();
        var answer = new FormAnswer { TextValue = "https://example.com" };

        model.SetAnswer(answer);

        model.TextInput.Should().Be("https://example.com");
        model.HasValue.Should().BeTrue();
    }

    [Fact]
    public void SetAnswer_ShouldSetHasValueToFalse_WhenAnswerIsNullAndRedirectFromCheckYourAnswerPage()
    {
        var model = new FormElementUrlInputModel();
        model.Initialize(new FormQuestion { IsRequired = false }, true);

        model.SetAnswer(null);

        model.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenHasValueIsNullAndIsNotRequired()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = false,
            HasValue = null
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);

        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Be("Select an option");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenTextInputIsMissingAndIsRequired()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = true,
            HasValue = true,
            TextInput = null
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);

        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Be("Enter a website address");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenTextInputIsInvalidUrl()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = true,
            HasValue = true,
            TextInput = "invalid-url"
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);

        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Be("Enter a valid website address in the correct format");
    }

    [Fact]
    public void Validate_ShouldPass_WhenValidUrlIsProvidedAndIsRequired()
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = true,
            HasValue = true,
            TextInput = "https://example.com"
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        var isValid = Validator.TryValidateObject(model, context, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }
}