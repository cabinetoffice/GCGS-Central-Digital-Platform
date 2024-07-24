using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementYesNoInputModelTest
{
    private readonly FormElementYesNoInputModel _model;

    public FormElementYesNoInputModelTest()
    {
        _model = new FormElementYesNoInputModel();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("yes", true)]
    [InlineData("no", false)]
    public void GetAnswer_GetsExpectedFormAnswer(string? input, bool? expected)
    {
        _model.YesNoInput = input;

        var answer = _model.GetAnswer();

        if (expected == null)
        {
            answer.Should().BeNull();
        }
        else
        {
            answer.Should().NotBeNull();
            answer!.BoolValue.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(true, "yes")]
    [InlineData(false, "no")]
    public void SetAnswer_SetsExpectedYesNoInput(bool? input, string? expected)
    {
        var answer = new FormAnswer { BoolValue = input };

        _model.SetAnswer(answer);

        if (expected == null)
        {
            _model.YesNoInput.Should().BeNull();
        }
        else
        {
            _model.YesNoInput.Should().NotBeNull();
            _model.YesNoInput!.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(true, null, "Please select an option.")]
    [InlineData(true, " ", "Please select an option.")]
    [InlineData(false, null, null)]
    [InlineData(false, "Some value", null)]
    public void Validate_ReturnsExpectedResults(bool isRequired, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.CurrentFormQuestionType = FormQuestionType.YesOrNo;
        _model.YesNoInput = textInput;

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