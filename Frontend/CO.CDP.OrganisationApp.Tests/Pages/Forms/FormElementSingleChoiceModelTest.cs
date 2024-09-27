using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementSingleChoiceModelTest
{
    private readonly FormElementSingleChoiceModel _model;

    public FormElementSingleChoiceModelTest()
    {
        _model = new FormElementSingleChoiceModel();
        _model.Options = new FormQuestionOptions() { Choices = ["Option 1", "Option 2", "Option 3"] } ;
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("yes", null)]
    [InlineData("no", null)]
    [InlineData("Option 1", "Option 1")]
    [InlineData("Option 2", "Option 2")]
    [InlineData("Option 3", "Option 3")]
    public void GetAnswer_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.SelectedOption = input;

        var answer = _model.GetAnswer();

        if (expected == null)
        {
            answer.Should().BeNull();
        }
        else
        {
            answer.Should().NotBeNull();
            answer!.OptionValue.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("yes", null)]
    [InlineData("no", null)]
    [InlineData("Option 1", "Option 1")]
    [InlineData("Option 2", "Option 2")]
    [InlineData("Option 3", "Option 3")]
    public void SetAnswer_SetsExpectedOption(string? selectedOption, string? expected)
    {
        var answer = new FormAnswer { OptionValue = selectedOption };

        _model.SetAnswer(answer);

        if (expected == null)
        {
            _model.SelectedOption.Should().BeNull();
        }
        else
        {
            _model.SelectedOption.Should().NotBeNull();
            _model.SelectedOption.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null, "Select an option")]
    [InlineData(" ", "Select an option")]
    [InlineData("yes", "Invalid option selected")]
    [InlineData("no", "Invalid option selected")]
    [InlineData("Option 1", null)]
    [InlineData("Option 2", null)]
    [InlineData("Option 3", null)]
    public void Validate_ReturnsExpectedResults(string? selectedOption, string? expectedErrorMessage)
    {
        _model.SelectedOption = selectedOption;

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