using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementYearInputModelTest
{
    private readonly FormElementYearInputModel _model = new();

    #region GetAnswer Tests

    [Fact]
    public void GetAnswer_WhenHasValueFalse_ReturnsFormAnswerWithBoolValueFalse()
    {
        _model.HasValue = false;
        _model.Year = "2023";

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(false);
        answer.As<FormAnswer>().TextValue.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_WhenHasValueTrue_AndYearIsValid_ReturnsFormAnswerWithTextValue()
    {
        _model.HasValue = true;
        _model.Year = "2023";

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(true);
        answer.As<FormAnswer>().TextValue.Should().Be("2023");
    }

    [Fact]
    public void GetAnswer_WhenHasValueTrue_AndYearIsNull_ReturnsFormAnswerWithoutTextValue()
    {
        _model.HasValue = true;
        _model.Year = null;

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(true);
        answer.As<FormAnswer>().TextValue.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_WhenHasValueNull_AndYearIsValid_ReturnsFormAnswerWithTextValue()
    {
        _model.HasValue = null;
        _model.Year = "2023";

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().BeNull();
        answer.As<FormAnswer>().TextValue.Should().Be("2023");
    }

    [Fact]
    public void GetAnswer_WhenHasValueNull_AndYearIsNull_ReturnsNull()
    {
        _model.HasValue = null;
        _model.Year = null;

        var answer = _model.GetAnswer();

        answer.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_WhenHasValueNull_AndYearIsEmpty_ReturnsNull()
    {
        _model.HasValue = null;
        _model.Year = "";

        var answer = _model.GetAnswer();

        answer.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_WhenHasValueNull_AndYearIsWhitespace_ReturnsNull()
    {
        _model.HasValue = null;
        _model.Year = "   ";

        var answer = _model.GetAnswer();

        answer.Should().BeNull();
    }

    #endregion

    #region SetAnswer Tests

    [Fact]
    public void SetAnswer_WhenAnswerIsNull_DoesNotSetProperties()
    {
        _model.Year = "2023";
        _model.HasValue = true;
        _model.SetAnswer(null);

        _model.Year.Should().Be("2023");
        _model.HasValue.Should().Be(true);
    }

    [Fact]
    public void SetAnswer_WhenAnswerContainsValues_SetsProperties()
    {
        var answer = new FormAnswer
        {
            BoolValue = true,
            TextValue = "2025"
        };

        _model.SetAnswer(answer);

        _model.HasValue.Should().Be(true);
        _model.Year.Should().Be("2025");
    }

    [Fact]
    public void SetAnswer_WhenAnswerContainsBoolValueOnly_SetsBoolValueOnly()
    {
        var answer = new FormAnswer
        {
            BoolValue = false,
            TextValue = null
        };

        _model.SetAnswer(answer);

        _model.HasValue.Should().Be(false);
        _model.Year.Should().BeNull();
    }

    [Fact]
    public void SetAnswer_WhenAnswerContainsTextValueOnly_SetsTextValueOnly()
    {
        var answer = new FormAnswer
        {
            BoolValue = null,
            TextValue = "2024"
        };

        _model.SetAnswer(answer);

        _model.HasValue.Should().BeNull();
        _model.Year.Should().Be("2024");
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_WhenQuestionTypeIsNotYear_ReturnsNoValidationErrors()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.IsRequired = true;
        _model.Year = null;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenRequired_AndYearIsNull_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = null;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year is required.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Fact]
    public void Validate_WhenRequired_AndYearIsEmpty_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = "";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year is required.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Fact]
    public void Validate_WhenRequired_AndYearIsWhitespace_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = "   ";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year is required.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Fact]
    public void Validate_WhenRequired_AndYearIsValid_ReturnsNoValidationErrors()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = "2023";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenRequired_AndYearHasInvalidFormat_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = "abc";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year entered is invalid. Please enter a valid year.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("202A")]
    [InlineData("-2023")]
    public void Validate_WhenRequired_AndYearFormatIsInvalid_ReturnsValidationError(string year)
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = true;
        _model.Year = year;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year entered is invalid. Please enter a valid year.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Fact]
    public void Validate_WhenNotRequired_AndHasValueIsNull_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = false;
        _model.HasValue = null;
        _model.Year = null;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Select an option");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.HasValue));
    }

    [Fact]
    public void Validate_WhenNotRequired_AndHasValueIsFalse_ReturnsNoValidationErrors()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = false;
        _model.HasValue = false;
        _model.Year = null;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenNotRequired_AndHasValueIsTrue_AndYearIsNull_ReturnsValidationError()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = false;
        _model.HasValue = true;
        _model.Year = null;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The year is required.");
        validationResults[0].MemberNames.Should().Contain(nameof(FormElementYearInputModel.Year));
    }

    [Fact]
    public void Validate_WhenNotRequired_AndHasValueIsTrue_AndYearIsValid_ReturnsNoValidationErrors()
    {
        _model.CurrentFormQuestionType = FormQuestionType.Year;
        _model.IsRequired = false;
        _model.HasValue = true;
        _model.Year = "2023";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }

    #endregion
}