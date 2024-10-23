using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementUrlInputModelTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("https://example.com")]
    public void GetAnswer_WhenHasValueFalse_GetsExpectedFormAnswer(string? input)
    {
        var model = new FormElementUrlInputModel
        {
            HasValue = false,
            TextInput = input
        };

        var answer = model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(false);
        answer.As<FormAnswer>().TextValue.Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("https://example.com", "https://example.com")]
    public void GetAnswer_WhenHasValueTrue_GetsExpectedFormAnswer(string? input, string? expected)
    {
        var model = new FormElementUrlInputModel
        {
            HasValue = true,
            TextInput = input
        };

        var answer = model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(true);
        answer.As<FormAnswer>().TextValue.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("https://example.com", "https://example.com")]
    public void GetAnswer_WhenHasValueIsNull_GetsExpectedFormAnswer(string? input, string? expected)
    {
        var model = new FormElementUrlInputModel
        {
            HasValue = null,
            TextInput = input
        };

        var answer = model.GetAnswer();

        if (expected == null)
        {
            answer.Should().BeNull();
        }
        else
        {
            answer.Should().NotBeNull();
            answer.As<FormAnswer>().BoolValue.Should().BeNull();
            answer.As<FormAnswer>().TextValue.Should().Be(expected);
        }
    }

    [Fact]
    public void SetAnswer_ShouldSetTextInputAndHasValue_WhenAnswerIsProvided()
    {
        var model = new FormElementUrlInputModel();
        var answer = new FormAnswer { TextValue = "https://example.com", BoolValue = true };

        model.SetAnswer(answer);

        model.TextInput.Should().Be("https://example.com");
        model.HasValue.Should().BeTrue();
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

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("http://example.url/has space")]
    public void Validate_ShouldReturnError_WhenTextInputIsInvalidUrl(string url)
    {
        var model = new FormElementUrlInputModel
        {
            IsRequired = true,
            HasValue = true,
            TextInput = url
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