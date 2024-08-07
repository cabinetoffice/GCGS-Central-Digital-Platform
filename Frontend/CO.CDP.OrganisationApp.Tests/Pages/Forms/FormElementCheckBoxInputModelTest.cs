using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;
public class FormElementCheckBoxInputModelTest
{
    private readonly FormElementCheckBoxInputModel _model;
    public FormElementCheckBoxInputModelTest()
    {
        _model = new FormElementCheckBoxInputModel();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, null)]
    [InlineData(true, true)]
    public void GetAnswer_GetsExpectedFormAnswer(bool? input, bool? expected)
    {
        _model.CheckBoxInput = input;

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
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void SetAnswer_SetsExpectedCheckBoxInput(bool? input, bool? expected)
    {
        var answer = new FormAnswer { BoolValue = input };

        _model.SetAnswer(answer);

        if (expected == null)
        {
            _model.CheckBoxInput.Should().BeNull();
        }
        else
        {
            _model.CheckBoxInput.Should().NotBeNull();
            _model.CheckBoxInput!.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(true, null, "You must agree to the declaration statements to proceed.")]
    [InlineData(true, false, "You must agree to the declaration statements to proceed.")]
    [InlineData(false, null, null)]
    [InlineData(false, true, null)]
    public void Validate_ReturnsExpectedResults(bool isRequired, bool? checkBoxInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.CurrentFormQuestionType = FormQuestionType.CheckBox;
        _model.CheckBoxInput = checkBoxInput;

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