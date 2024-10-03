using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;
public class FormElementGroupedSingleChoiceModelTests
{
    private readonly FormElementGroupedSingleChoiceModel _model;

    public FormElementGroupedSingleChoiceModelTests()
    {
        _model = new FormElementGroupedSingleChoiceModel
        {
            Options = new FormQuestionOptions
            {
                Groups = new List<FormQuestionGroup>
                    {
                        new FormQuestionGroup
                        {
                            Name = "Group 1",
                            Hint = "Group 1 Hint",
                            Caption = "Group 1 Caption",
                            Choices = new List<FormQuestionGroupChoice>
                            {
                                new FormQuestionGroupChoice { Title = "Group Choice 1", Value = "group_choice_1" },
                                new FormQuestionGroupChoice { Title = "Group Choice 2", Value = "group_choice_2" }
                            }
                        }
                    }
            }
        };
    }

    [Fact]
    public void GetAnswer_ShouldReturnNull_WhenNoOptionIsSelected()
    {
        _model.SelectedOption = null;
        var answer = _model.GetAnswer();
        answer.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_ShouldReturnFormAnswer_WhenValidOptionIsSelected()
    {
        _model.SelectedOption = "group_choice_1";
        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer!.OptionValue.Should().Be("group_choice_1");
    }

    [Fact]
    public void GetAnswer_ShouldReturnNull_WhenInvalidOptionIsSelected()
    {
        _model.SelectedOption = "invalid_choice";

        var answer = _model.GetAnswer();
        answer.Should().BeNull();
    }

    [Fact]
    public void SetAnswer_ShouldSetSelectedOption_WhenValidOptionIsProvided()
    {
        var answer = new FormAnswer { OptionValue = "group_choice_2" };

        _model.SetAnswer(answer);
        _model.SelectedOption.Should().Be("group_choice_2");
    }

    [Fact]
    public void SetAnswer_ShouldNotSetSelectedOption_WhenInvalidOptionIsProvided()
    {
        var answer = new FormAnswer { OptionValue = "invalid_choice" };

        _model.SetAnswer(answer);
        _model.SelectedOption.Should().BeNull();
    }

    [Fact]
    public void Validate_ShouldReturnValidationError_WhenNoOptionIsSelected()
    {
        _model.SelectedOption = null;

        var validationResults = _model.Validate(new ValidationContext(_model));
        validationResults.Should().ContainSingle(v => v.ErrorMessage == "Select an option");
    }

    [Fact]
    public void Validate_ShouldReturnValidationError_WhenInvalidOptionIsSelected()
    {
        _model.SelectedOption = "invalid_choice";

        var validationResults = _model.Validate(new ValidationContext(_model));
        validationResults.Should().ContainSingle(v => v.ErrorMessage == "Invalid option selected");
    }

    [Fact]
    public void Validate_ShouldReturnNoError_WhenValidOptionIsSelected()
    {
        _model.SelectedOption = "group_choice_1";

        var validationResults = _model.Validate(new ValidationContext(_model));
        validationResults.Should().BeEmpty();
    }
}
