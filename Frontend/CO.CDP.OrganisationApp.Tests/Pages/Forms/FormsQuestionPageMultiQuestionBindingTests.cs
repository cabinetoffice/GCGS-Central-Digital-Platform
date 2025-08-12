using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsQuestionPageMultiQuestionBindingTests
{
    [Theory]
    [InlineData("option1")]
    [InlineData("option2")]
    [InlineData("")]
    public void SingleChoiceModel_ShouldBindSelectedOption_WhenFormContainsValue(string selectedOption)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementSingleChoiceModel { QuestionId = questionId };
        var fieldName = model.GetFieldName(nameof(model.SelectedOption));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(fieldName)).Returns(true);
        mockFormCollection.Setup(f => f[fieldName]).Returns(new StringValues(selectedOption));

        if (mockFormCollection.Object.ContainsKey(fieldName))
        {
            model.SelectedOption = mockFormCollection.Object[fieldName].ToString();
        }

        model.SelectedOption.Should().Be(selectedOption);
    }

    [Theory]
    [InlineData("Some text input", true)]
    [InlineData("", false)]
    [InlineData("Another text", true)]
    public void TextInputModel_ShouldBindTextInputAndHasValue_WhenFormContainsValues(string textInput, bool hasValue)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementTextInputModel { QuestionId = questionId };
        var textFieldName = model.GetFieldName(nameof(model.TextInput));
        var hasValueFieldName = model.GetFieldName(nameof(model.HasValue));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(textFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[textFieldName]).Returns(new StringValues(textInput));
        mockFormCollection.Setup(f => f.ContainsKey(hasValueFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[hasValueFieldName]).Returns(new StringValues(hasValue.ToString()));

        if (mockFormCollection.Object.ContainsKey(textFieldName))
        {
            model.TextInput = mockFormCollection.Object[textFieldName].ToString();
        }
        if (mockFormCollection.Object.ContainsKey(hasValueFieldName))
        {
            model.HasValue = bool.Parse(mockFormCollection.Object[hasValueFieldName].ToString());
        }

        model.TextInput.Should().Be(textInput);
        model.HasValue.Should().Be(hasValue);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("", false)]
    [InlineData("https://another-site.com", true)]
    public void UrlInputModel_ShouldBindTextInputAndHasValue_WhenFormContainsValues(string urlInput, bool hasValue)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementUrlInputModel { QuestionId = questionId };
        var textFieldName = model.GetFieldName(nameof(model.TextInput));
        var hasValueFieldName = model.GetFieldName(nameof(model.HasValue));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(textFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[textFieldName]).Returns(new StringValues(urlInput));
        mockFormCollection.Setup(f => f.ContainsKey(hasValueFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[hasValueFieldName]).Returns(new StringValues(hasValue.ToString()));

        if (mockFormCollection.Object.ContainsKey(textFieldName))
        {
            model.TextInput = mockFormCollection.Object[textFieldName].ToString();
        }
        if (mockFormCollection.Object.ContainsKey(hasValueFieldName))
        {
            model.HasValue = bool.Parse(mockFormCollection.Object[hasValueFieldName].ToString());
        }

        model.TextInput.Should().Be(urlInput);
        model.HasValue.Should().Be(hasValue);
    }

    [Theory]
    [InlineData("15", "12", "2023", true)]
    [InlineData("", "", "", false)]
    [InlineData("01", "01", "2024", true)]
    public void DateInputModel_ShouldBindDateFieldsAndHasValue_WhenFormContainsValues(string day, string month, string year, bool hasValue)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementDateInputModel { QuestionId = questionId };
        var dayFieldName = model.GetFieldName(nameof(model.Day));
        var monthFieldName = model.GetFieldName(nameof(model.Month));
        var yearFieldName = model.GetFieldName(nameof(model.Year));
        var hasValueFieldName = model.GetFieldName(nameof(model.HasValue));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(dayFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[dayFieldName]).Returns(new StringValues(day));
        mockFormCollection.Setup(f => f.ContainsKey(monthFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[monthFieldName]).Returns(new StringValues(month));
        mockFormCollection.Setup(f => f.ContainsKey(yearFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[yearFieldName]).Returns(new StringValues(year));
        mockFormCollection.Setup(f => f.ContainsKey(hasValueFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[hasValueFieldName]).Returns(new StringValues(hasValue.ToString()));

        if (mockFormCollection.Object.ContainsKey(dayFieldName))
            model.Day = mockFormCollection.Object[dayFieldName].ToString();
        if (mockFormCollection.Object.ContainsKey(monthFieldName))
            model.Month = mockFormCollection.Object[monthFieldName].ToString();
        if (mockFormCollection.Object.ContainsKey(yearFieldName))
            model.Year = mockFormCollection.Object[yearFieldName].ToString();
        if (mockFormCollection.Object.ContainsKey(hasValueFieldName))
            model.HasValue = bool.Parse(mockFormCollection.Object[hasValueFieldName].ToString());

        model.Day.Should().Be(day);
        model.Month.Should().Be(month);
        model.Year.Should().Be(year);
        model.HasValue.Should().Be(hasValue);
    }

    [Theory]
    [InlineData("test-file.pdf", true)]
    [InlineData("", false)]
    [InlineData("document.docx", true)]
    public void FileUploadModel_ShouldBindUploadedFileNameAndHasValue_WhenFormContainsValues(string uploadedFileName, bool hasValue)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementFileUploadModel { QuestionId = questionId };
        var uploadedFileNameFieldName = model.GetFieldName(nameof(model.UploadedFileName));
        var hasValueFieldName = model.GetFieldName(nameof(model.HasValue));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(uploadedFileNameFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[uploadedFileNameFieldName]).Returns(new StringValues(uploadedFileName));
        mockFormCollection.Setup(f => f.ContainsKey(hasValueFieldName)).Returns(true);
        mockFormCollection.Setup(f => f[hasValueFieldName]).Returns(new StringValues(hasValue.ToString()));

        if (mockFormCollection.Object.ContainsKey(uploadedFileNameFieldName))
        {
            model.UploadedFileName = mockFormCollection.Object[uploadedFileNameFieldName].ToString();
        }
        if (mockFormCollection.Object.ContainsKey(hasValueFieldName))
        {
            model.HasValue = bool.Parse(mockFormCollection.Object[hasValueFieldName].ToString());
        }

        model.UploadedFileName.Should().Be(uploadedFileName);
        model.HasValue.Should().Be(hasValue);
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("no")]
    [InlineData("")]
    public void YesNoInputModel_ShouldBindYesNoInput_WhenFormContainsValue(string yesNoInput)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementYesNoInputModel { QuestionId = questionId };
        var fieldName = model.GetFieldName(nameof(model.YesNoInput));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(fieldName)).Returns(true);
        mockFormCollection.Setup(f => f[fieldName]).Returns(new StringValues(yesNoInput));

        if (mockFormCollection.Object.ContainsKey(fieldName))
        {
            model.YesNoInput = mockFormCollection.Object[fieldName].ToString();
        }

        model.YesNoInput.Should().Be(yesNoInput);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CheckBoxInputModel_ShouldBindCheckBoxInput_WhenFormContainsValue(bool checkBoxInput)
    {
        var questionId = Guid.NewGuid();
        var model = new FormElementCheckBoxInputModel { QuestionId = questionId };
        var fieldName = model.GetFieldName(nameof(model.CheckBoxInput));

        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(f => f.ContainsKey(fieldName)).Returns(true);
        mockFormCollection.Setup(f => f[fieldName]).Returns(new StringValues(checkBoxInput.ToString()));

        if (mockFormCollection.Object.ContainsKey(fieldName))
        {
            model.CheckBoxInput = bool.Parse(mockFormCollection.Object[fieldName].ToString());
        }

        model.CheckBoxInput.Should().Be(checkBoxInput);
    }
}