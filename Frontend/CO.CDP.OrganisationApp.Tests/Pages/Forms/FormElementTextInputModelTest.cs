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
    [InlineData(true, null, "Please provide a value.")]
    [InlineData(true, " ", "Please provide a value.")]
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
}