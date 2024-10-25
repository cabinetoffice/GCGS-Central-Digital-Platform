using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementTextInputModelTest
{
    private readonly FormElementTextInputModel _model = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("Test value")]
    public void GetAnswer_WhenHasValueFalse_GetsExpectedFormAnswer(string? input)
    {
        _model.HasValue = false;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(false);
        answer.As<FormAnswer>().TextValue.Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Test value", "Test value")]
    public void GetAnswer_WhenHasValueTrue_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.HasValue = true;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(true);
        answer.As<FormAnswer>().TextValue.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Test value", "Test value")]
    public void GetAnswer_WhenHasValueIsNull_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.HasValue = null;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

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

    [Theory]
    [InlineData(null, null)]
    [InlineData("Test value", "Test value")]
    public void SetAnswer_SetsExpectedTextInput(string? input, string? expected)
    {
        var answer = new FormAnswer { TextValue = input };

        _model.SetAnswer(answer);

        if (expected == null)
        {
            _model.TextInput.Should().BeNull();
        }
        else
        {
            _model.TextInput.Should().NotBeNull();
            _model.TextInput!.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(true, null, null, "Enter a value")]
    [InlineData(true, null, " ", "Enter a value")]
    [InlineData(false, false, null, null)]
    [InlineData(false, true, "Some value", null)]
    public void Validate_ReturnsExpectedResults(bool isRequired, bool? hasValue, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.HasValue = hasValue;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.TextInput = textInput;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("invalid-email", "Enter an email address in the correct format, like name@example.com.")]
    [InlineData("valid.email@example.com", null)]
    public void Validate_EmailValidation_ReturnsExpectedResults(string? email, string? expectedErrorMessage)
    {
        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.Heading = "Email Address";
        _model.TextInput = email;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(true, null, "Email Address", null, "Enter an email address in the correct format, like name@example.com.")]
    [InlineData(true, null, "Email Address", "invalid-email", "Enter an email address in the correct format, like name@example.com.")]
    [InlineData(true, null, "Email Address", "valid.email@example.com", null)]
    [InlineData(false, true, "Email Address", "somevalue", "Enter an email address in the correct format, like name@example.com.")]
    [InlineData(false, false, "Email Address", null, null)]
    [InlineData(false, true, "Name", "invalid-email", null)]
    public void Validate_EmailFieldValidationBasedOnHeading_ReturnsExpectedResults(bool isRequired, bool? hasValue, string heading, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.HasValue = hasValue;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.Heading = heading;
        _model.TextInput = textInput;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }
}