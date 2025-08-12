using CO.CDP.AwsServices;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Reflection;
using FormAnswer = CO.CDP.OrganisationApp.Models.FormAnswer;
using FormQuestion = CO.CDP.OrganisationApp.Models.FormQuestion;
using FormQuestionOptions = CO.CDP.OrganisationApp.Models.FormQuestionOptions;
using FormQuestionType = CO.CDP.OrganisationApp.Models.FormQuestionType;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsQuestionPageValidationTests
{
    private readonly FormsQuestionPageModel _page;
    private readonly ModelStateDictionary _modelState;

    public FormsQuestionPageValidationTests()
    {
        var tempDataServiceMock = new Mock<ITempDataService>();
        var formsEngineMock = new Mock<IFormsEngine>();
        var choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        var organisationClientMock = new Mock<IOrganisationClient>();
        var userInfoServiceMock = new Mock<IUserInfoService>();

        _page = new FormsQuestionPageModel(
            Mock.Of<IPublisher>(),
            formsEngineMock.Object,
            tempDataServiceMock.Object,
            Mock.Of<IFileHostManager>(),
            organisationClientMock.Object,
            userInfoServiceMock.Object,
            Mock.Of<IAnswerDisplayService>());

        _modelState = new ModelStateDictionary();

        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new(), new(), _modelState);
        var pageContext = new PageContext(actionContext);
        _page.PageContext = pageContext;

        _page.OrganisationId = Guid.NewGuid();
        _page.FormId = Guid.NewGuid();
        _page.SectionId = Guid.NewGuid();
        _page.CurrentQuestionId = Guid.NewGuid();
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapAddressErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Address, "Address Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].AddressLine1", "Address line 1 is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Postcode", "Postcode is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_AddressLine1");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Postcode");
        _modelState[$"Q_{actualQuestionId}_AddressLine1"]!.Errors.Should().Contain(e => e.ErrorMessage == "Address line 1 is required");
        _modelState[$"Q_{actualQuestionId}_Postcode"]!.Errors.Should().Contain(e => e.ErrorMessage == "Postcode is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleMixedQuestionTypes_WhenMultiQuestionModelHasErrors()
    {
        var textQuestionId = Guid.NewGuid();
        var yesNoQuestionId = Guid.NewGuid();
        var dateQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(textQuestionId, FormQuestionType.Text, "Text Question"),
            CreateFormQuestion(yesNoQuestionId, FormQuestionType.YesOrNo, "Yes/No Question"),
            CreateFormQuestion(dateQuestionId, FormQuestionType.Date, "Date Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var firstQuestionId = questionModels[0].QuestionId!.Value;
        var secondQuestionId = questionModels[1].QuestionId!.Value;
        var thirdQuestionId = questionModels[2].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].TextInput", "Text is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[1].YesNoInput", "Selection is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[2].Day", "Day is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{firstQuestionId}_TextInput");
        _modelState.Should().ContainKey($"Q_{secondQuestionId}_YesNoInput");
        _modelState.Should().ContainKey($"Q_{thirdQuestionId}_Day");
        _modelState[$"Q_{firstQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Text is required");
        _modelState[$"Q_{secondQuestionId}_YesNoInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
        _modelState[$"Q_{thirdQuestionId}_Day"]!.Errors.Should().Contain(e => e.ErrorMessage == "Day is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapSingleChoiceErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.SingleChoice, "Single Choice Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].SelectedOption", "Please select an option");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_SelectedOption");
        _modelState[$"Q_{actualQuestionId}_SelectedOption"]!.Errors.Should().Contain(e => e.ErrorMessage == "Please select an option");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapGroupedSingleChoiceErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.GroupedSingleChoice, "Grouped Single Choice Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].SelectedOption", "Please select an option from the group");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_SelectedOption");
        _modelState[$"Q_{actualQuestionId}_SelectedOption"]!.Errors.Should().Contain(e => e.ErrorMessage == "Please select an option from the group");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapMultiLineInputErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.MultiLine, "MultiLine Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].TextInput", "MultiLine text is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_TextInput");
        _modelState[$"Q_{actualQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "MultiLine text is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapUrlInputErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Url, "Url Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].TextInput", "Url is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_TextInput");
        _modelState[$"Q_{actualQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Url is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapCheckBoxInputErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.CheckBox, "CheckBox Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].CheckBoxInput", "Checkbox must be checked");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_CheckBoxInput");
        _modelState[$"Q_{actualQuestionId}_CheckBoxInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Checkbox must be checked");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapFileUploadErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.FileUpload, "FileUpload Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].UploadedFile", "File upload is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_UploadedFile");
        _modelState[$"Q_{actualQuestionId}_UploadedFile"]!.Errors.Should().Contain(e => e.ErrorMessage == "File upload is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapDateMonthAndYearErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Date, "Date Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].Month", "Month is required");
        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].Year", "Year is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Month");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Year");
        _modelState[$"Q_{actualQuestionId}_Month"]!.Errors.Should().Contain(e => e.ErrorMessage == "Month is required");
        _modelState[$"Q_{actualQuestionId}_Year"]!.Errors.Should().Contain(e => e.ErrorMessage == "Year is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldNotAffectOtherErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Text, "Text Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var actualQuestionId = multiQuestionModel.QuestionModels.ToArray()[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].TextInput", "Text is required");
        _modelState.AddModelError("SomeOtherField", "This is an unrelated error");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_TextInput");
        _modelState.Should().ContainKey("SomeOtherField");
        _modelState[$"Q_{actualQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Text is required");
        _modelState["SomeOtherField"]!.Errors.Should().Contain(e => e.ErrorMessage == "This is an unrelated error");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldDoNothing_WhenNoErrorsExist()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Text, "Text Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().BeEmpty();
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldDoNothing_WhenMultiQuestionViewModelIsNull()
    {
        _page.MultiQuestionViewModel = null;

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().BeEmpty();
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleSingleChoiceQuestionType()
    {
        var singleChoiceQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(singleChoiceQuestionId, FormQuestionType.SingleChoice, "Single Choice Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].SingleChoiceInput", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_SingleChoiceInput");
        _modelState[$"Q_{actualQuestionId}_SingleChoiceInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleGroupedSingleChoiceQuestionType()
    {
        var groupedSingleChoiceQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(groupedSingleChoiceQuestionId, FormQuestionType.GroupedSingleChoice, "Grouped Single Choice Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].GroupedSingleChoiceInput", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_GroupedSingleChoiceInput");
        _modelState[$"Q_{actualQuestionId}_GroupedSingleChoiceInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleMultiLineInputQuestionType()
    {
        var multiLineInputQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(multiLineInputQuestionId, FormQuestionType.MultiLine, "Multi Line Input Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].MultiLineInput", "Input is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_MultiLineInput");
        _modelState[$"Q_{actualQuestionId}_MultiLineInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Input is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleUrlInputQuestionType()
    {
        var urlInputQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(urlInputQuestionId, FormQuestionType.Url, "URL Input Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].UrlInput", "URL is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_UrlInput");
        _modelState[$"Q_{actualQuestionId}_UrlInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "URL is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleCheckBoxInputQuestionType()
    {
        var checkBoxInputQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(checkBoxInputQuestionId, FormQuestionType.CheckBox, "Check Box Input Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].CheckBoxInput", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_CheckBoxInput");
        _modelState[$"Q_{actualQuestionId}_CheckBoxInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleFileUploadQuestionType()
    {
        var fileUploadQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(fileUploadQuestionId, FormQuestionType.FileUpload, "File Upload Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].FileUpload", "File is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_FileUpload");
        _modelState[$"Q_{actualQuestionId}_FileUpload"]!.Errors.Should().Contain(e => e.ErrorMessage == "File is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldHandleDateMonthYearQuestionType()
    {
        var dateMonthYearQuestionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(dateMonthYearQuestionId, FormQuestionType.Date, "Date Month/Year Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Month", "Month is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Year", "Year is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Month");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Year");
        _modelState[$"Q_{actualQuestionId}_Month"]!.Errors.Should().Contain(e => e.ErrorMessage == "Month is required");
        _modelState[$"Q_{actualQuestionId}_Year"]!.Errors.Should().Contain(e => e.ErrorMessage == "Year is required");
    }

    [Theory]
    [InlineData(FormQuestionType.Text, "HasValue", "Text input requires selection")]
    [InlineData(FormQuestionType.Url, "HasValue", "URL input requires selection")]
    [InlineData(FormQuestionType.Date, "HasValue", "Date input requires selection")]
    [InlineData(FormQuestionType.FileUpload, "HasValue", "File upload requires selection")]
    public void MapMultiQuestionValidationErrors_ShouldMapOptionalQuestionHasValueErrors_WhenMultiQuestionModelHasErrors(FormQuestionType questionType, string propertyName, string errorMessage)
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, questionType, $"{questionType} Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError($"MultiQuestionViewModel.QuestionModels[0].{propertyName}", errorMessage);

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_{propertyName}");
        _modelState[$"Q_{actualQuestionId}_{propertyName}"]!.Errors.Should().Contain(e => e.ErrorMessage == errorMessage);
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapTextInputOptionalQuestionErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Text, "Text Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].TextInput", "Text is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].HasValue", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_TextInput");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_HasValue");
        _modelState[$"Q_{actualQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "Text is required");
        _modelState[$"Q_{actualQuestionId}_HasValue"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapUrlInputOptionalQuestionErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Url, "URL Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].TextInput", "URL is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].HasValue", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_TextInput");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_HasValue");
        _modelState[$"Q_{actualQuestionId}_TextInput"]!.Errors.Should().Contain(e => e.ErrorMessage == "URL is required");
        _modelState[$"Q_{actualQuestionId}_HasValue"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapDateInputOptionalQuestionErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.Date, "Date Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Day", "Day is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Month", "Month is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].Year", "Year is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].HasValue", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Day");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Month");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_Year");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_HasValue");
        _modelState[$"Q_{actualQuestionId}_Day"]!.Errors.Should().Contain(e => e.ErrorMessage == "Day is required");
        _modelState[$"Q_{actualQuestionId}_Month"]!.Errors.Should().Contain(e => e.ErrorMessage == "Month is required");
        _modelState[$"Q_{actualQuestionId}_Year"]!.Errors.Should().Contain(e => e.ErrorMessage == "Year is required");
        _modelState[$"Q_{actualQuestionId}_HasValue"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    [Fact]
    public void MapMultiQuestionValidationErrors_ShouldMapFileUploadOptionalQuestionErrors_WhenMultiQuestionModelHasErrors()
    {
        var questionId = Guid.NewGuid();

        var multiQuestionModel = CreateMultiQuestionModel(new[]
        {
            CreateFormQuestion(questionId, FormQuestionType.FileUpload, "File Upload Question")
        });

        _page.MultiQuestionViewModel = multiQuestionModel;

        var questionModels = multiQuestionModel.QuestionModels.ToArray();
        var actualQuestionId = questionModels[0].QuestionId!.Value;

        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].UploadedFileName", "File name is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].UploadedFile", "File is required");
        _modelState.AddModelError("MultiQuestionViewModel.QuestionModels[0].HasValue", "Selection is required");

        InvokeMapMultiQuestionValidationErrors();

        _modelState.Should().ContainKey($"Q_{actualQuestionId}_UploadedFileName");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_UploadedFile");
        _modelState.Should().ContainKey($"Q_{actualQuestionId}_HasValue");
        _modelState[$"Q_{actualQuestionId}_UploadedFileName"]!.Errors.Should().Contain(e => e.ErrorMessage == "File name is required");
        _modelState[$"Q_{actualQuestionId}_UploadedFile"]!.Errors.Should().Contain(e => e.ErrorMessage == "File is required");
        _modelState[$"Q_{actualQuestionId}_HasValue"]!.Errors.Should().Contain(e => e.ErrorMessage == "Selection is required");
    }

    private void InvokeMapMultiQuestionValidationErrors()
    {
        var method = typeof(FormsQuestionPageModel).GetMethod("MapMultiQuestionValidationErrors",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(ModelStateDictionary) },
            null);

        if (method == null)
        {
            return;
        }

        if (_page.MultiQuestionViewModel != null)
        {
        }
        method.Invoke(_page, new object[] { _modelState });
    }

    private FormElementMultiQuestionModel CreateMultiQuestionModel(FormQuestion[] questions)
    {
        var multiQuestionPage = new MultiQuestionPageModel
        {
            Questions = questions.ToList()
        };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());
        return multiQuestionModel;
    }

    private FormQuestion CreateFormQuestion(Guid id, FormQuestionType type, string title)
    {
        return new FormQuestion
        {
            Id = id,
            Type = type,
            Title = title,
            Options = new FormQuestionOptions()
        };
    }
}

