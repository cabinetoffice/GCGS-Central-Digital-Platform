using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementTextInputModelTest
{
    private readonly FormElementTextInputModel _model;

    public FormElementTextInputModelTest()
    {
        _model = new FormElementTextInputModel();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Test value", "Test value")]
    public void GetAnswer_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        if (expected == null)
        {
            answer.Should().BeNull();
        }
        else
        {
            answer.Should().NotBeNull();
            answer!.TextValue.Should().Be(expected);
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
    [InlineData(true, null, "All information is required on this page.")]
    [InlineData(true, " ", "All information is required on this page.")]
    [InlineData(false, null, null)]
    [InlineData(false, "Some value", null)]
    public void Validate_ReturnsExpectedResults(bool isRequired, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
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
        _model.IsRequired = false;
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
    [InlineData(true, "Email Address", null, "All information is required on this page.")]
    [InlineData(true, "Email Address", "invalid-email", "Enter an email address in the correct format, like name@example.com.")]
    [InlineData(true, "Email Address", "valid.email@example.com", null)]
    [InlineData(false, "Email Address", "somevalue", "Enter an email address in the correct format, like name@example.com.")]
    [InlineData(false, "Email Address", null, null)]
    [InlineData(false, "Name", "invalid-email", null)]
    public void Validate_EmailFieldValidationBasedOnHeading_ReturnsExpectedResults(bool isRequired, string heading, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
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