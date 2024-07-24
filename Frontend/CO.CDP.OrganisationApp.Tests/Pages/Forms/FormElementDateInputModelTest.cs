using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementDateInputModelTest
{
    private readonly FormElementDateInputModel _model;

    public FormElementDateInputModelTest()
    {
        _model = new FormElementDateInputModel();
    }

    [Theory]
    [InlineData("01", "12", null)]
    [InlineData(null, "12", "2023")]
    [InlineData("01", null, "2023")]
    public void GetAnswer_ReturnsNull_WhenDateComponentsAreIncomplete(string? day, string? month, string? year)
    {
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;

        var answer = _model.GetAnswer();

        answer.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_ReturnsAnswer_WhenDateComponentsAreComplete()
    {
        _model.Day = "01";
        _model.Month = "12";
        _model.Year = "2023";

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer!.DateValue.Should().Be(new DateTime(2023, 12, 1));
    }

    [Fact]
    public void SetAnswer_SetsDateComponents_WhenAnswerIsValid()
    {
        var answer = new FormAnswer { DateValue = new DateTime(2023, 12, 1) };

        _model.SetAnswer(answer);

        _model.Day.Should().Be("1");
        _model.Month.Should().Be("12");
        _model.Year.Should().Be("2023");
    }

    [Fact]
    public void SetAnswer_DoesNotSetDateComponents_WhenAnswerIsNull()
    {
        _model.SetAnswer(null);

        _model.Day.Should().BeNull();
        _model.Month.Should().BeNull();
        _model.Year.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "12", "2023", "Date must include a day")]
    [InlineData("32", "12", "2023", "Day must be a valid number")]
    [InlineData("11", null, "2023", "Date must include a month")]
    [InlineData("01", "13", "2023", "Month must be a valid number")]
    [InlineData("11", "11", null, "Date must include a year")]
    [InlineData("01", "12", "abcd", "Year must be a valid number")]
    [InlineData("31", "02", "2023", "Date must be a real date")]
    [InlineData("01", "12", "2024", "Date must be today or in the past")]
    public void Validate_ReturnsError_WhenDateIsInvalid(string? day, string? month, string? year, string expectedErrorMessage)
    {
        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.Date;
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenDateIsValid()
    {
        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.Date;
        _model.Day = "01";
        _model.Month = "12";
        _model.Year = "2023";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }
}