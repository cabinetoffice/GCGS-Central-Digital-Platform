using CO.CDP.AwsServices;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsQuestionPageModelTest
{
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<IFileHostManager> _fileHostManagerMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IUserInfoService> _userInfoServiceMock;
    private readonly Mock<CO.CDP.Forms.WebApiClient.IFormsClient> _formsApiClientMock;
    private readonly Mock<DataSharing.WebApiClient.IDataSharingClient> _dataSharingClientMock;
    private readonly Mock<IChoiceProviderService> _choiceProviderServiceMock;
    private readonly Mock<IAnswerDisplayService> _answerDisplayServiceMock;
    private readonly FormsQuestionPageModel _pageModel;
    private readonly Guid _textQuestionId = Guid.NewGuid();
    private readonly Mock<IObjectModelValidator> _objectModelValidatorMock;

    public FormsQuestionPageModelTest()
    {
        _formsEngineMock = new Mock<IFormsEngine>();
        _choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _formsApiClientMock = new Mock<CO.CDP.Forms.WebApiClient.IFormsClient>();
        _dataSharingClientMock = new Mock<DataSharing.WebApiClient.IDataSharingClient>();
        _answerDisplayServiceMock = new Mock<IAnswerDisplayService>();

        _fileHostManagerMock = new Mock<IFileHostManager>();
        _publisherMock = new Mock<IPublisher>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _userInfoServiceMock = new Mock<IUserInfoService>();
        _tempDataServiceMock = new Mock<ITempDataService>();

        var form = new SectionQuestionsResponse
        {
            Questions =
            [
                new FormQuestion
                {
                    Id = _textQuestionId, Type = FormQuestionType.Text, SummaryTitle = "Sample Question",
                    Options = new FormQuestionOptions()
                }
            ]
        };

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(form);
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());
        _tempDataServiceMock.Setup(t => t.Remove(It.IsAny<string>()));

        _pageModel = new FormsQuestionPageModel(
            _publisherMock.Object,
            _formsEngineMock.Object,
            _tempDataServiceMock.Object,
            _fileHostManagerMock.Object,
            _organisationClientMock.Object,
            _userInfoServiceMock.Object,
            _answerDisplayServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();

        _objectModelValidatorMock = new Mock<IObjectModelValidator>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IObjectModelValidator)))
            .Returns(_objectModelValidatorMock.Object);
        httpContext.RequestServices = serviceProviderMock.Object;

        _pageModel.PageContext = new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(modelMetadataProvider, modelState)
        };

        _pageModel.OrganisationId = Guid.NewGuid();
        _pageModel.FormId = Guid.NewGuid();
        _pageModel.SectionId = Guid.NewGuid();
    }

    [Fact]
    public async Task OnGetAsync_RedirectsToPageNotFound_WhenCurrentQuestionIsNull()
    {
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync((FormQuestion?)null);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetAsync_ReturnsPage_WhenCurrentQuestionIsNotNull()
    {
        var formQuestion = new FormQuestion
        {
            Id = Guid.NewGuid(), Type = FormQuestionType.Text, NextQuestion = Guid.NewGuid()
        };
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _pageModel.PartialViewName.Should().Be("_FormElementTextInput");
        _pageModel.PartialViewModel.Should().NotBeNull();
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToPageNotFound_WhenCurrentQuestionIsNull()
    {
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync((FormQuestion?)null);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task EncType_ReturnsMultipartFormData_WhenCurrentFormQuestionTypeIsFileUpload()
    {
        var formQuestion = new FormQuestion
        {
            Id = Guid.NewGuid(), Type = FormQuestionType.FileUpload, NextQuestion = Guid.NewGuid(),
            NextQuestionAlternative = Guid.NewGuid()
        };
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        await _pageModel.OnGetAsync();

        _pageModel.EncType.Should().Be("multipart/form-data");
    }

    [Fact]
    public async Task EncType_ReturnsUrlEncoded_WhenCurrentFormQuestionTypeIsNotFileUpload()
    {
        var formQuestion = new FormQuestion
        {
            Id = Guid.NewGuid(), Type = FormQuestionType.Text, NextQuestion = Guid.NewGuid()
        };
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        await _pageModel.OnGetAsync();

        _pageModel.EncType.Should().Be("application/x-www-form-urlencoded");
    }

    [Fact]
    public async Task GetAnswers_ShouldReturnCheckYouAnswersSummaries()
    {
        var answerSet = new FormQuestionAnswerState
        {
            Answers =
            [
                new QuestionAnswer
                {
                    QuestionId = _textQuestionId,
                    Answer = new FormAnswer { TextValue = "Sample Answer" }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerSet);

        _answerDisplayServiceMock.Setup(a => a.FormatAnswerForDisplayAsync(It.IsAny<QuestionAnswer>(), It.IsAny<FormQuestion>()))
            .ReturnsAsync("Sample Answer");

        var answers = (await _pageModel.GetAnswers()).ToList();

        answers.Should().HaveCount(1);
        answers.First().Answer.Should().Be("Sample Answer");
        answers.First().Title.Should().Be("Sample Question");
    }

    [Fact]
    public async Task GetAnswers_ShouldReturnLocalizedYesForTrueBoolAnswer()
    {
        var answerSet = new FormQuestionAnswerState
        {
            Answers =
            [
                new QuestionAnswer
                {
                    QuestionId = _textQuestionId,
                    Answer = new FormAnswer { BoolValue = true }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerSet);

        _answerDisplayServiceMock.Setup(a => a.FormatAnswerForDisplayAsync(It.IsAny<QuestionAnswer>(), It.IsAny<FormQuestion>()))
            .ReturnsAsync("Yes");

        var answers = (await _pageModel.GetAnswers()).ToList();

        answers.Should().HaveCount(1);
        answers.First().Answer.Should().Be("Yes");
    }

    [Fact]
    public async Task GetAnswers_ShouldReturnLocalizedNoForFalseBoolAnswer()
    {
        var answerSet = new FormQuestionAnswerState
        {
            Answers =
            [
                new QuestionAnswer
                {
                    QuestionId = _textQuestionId,
                    Answer = new FormAnswer { BoolValue = false }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerSet);

        _answerDisplayServiceMock.Setup(a => a.FormatAnswerForDisplayAsync(It.IsAny<QuestionAnswer>(), It.IsAny<FormQuestion>()))
            .ReturnsAsync("No");

        var answers = (await _pageModel.GetAnswers()).ToList();

        answers.Should().HaveCount(1);
        answers.First().Answer.Should().Be("No");
    }

    [Fact]
    public async Task
        OnPostAsync_ShouldCallSaveUpdateAnswersAndRedirectToFormsAnswerSetSummary_WhenCurrentQuestionIsCheckYourAnswerQuestion()
    {
        var currentQuestionId = Guid.NewGuid();
        var checkYourAnswerQuestionId = currentQuestionId;
        var formQuestion = new FormQuestion
        {
            Id = currentQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new SectionQuestionsResponse
            {
                Questions =
                [
                    new()
                    {
                        Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
                    }
                ]
            });

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());
        var result = await _pageModel.OnPostAsync();
        _formsEngineMock.Verify(
            f => f.SaveUpdateAnswers(_pageModel.FormId, _pageModel.SectionId, _pageModel.OrganisationId,
                It.IsAny<FormQuestionAnswerState>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("FormsAnswerSetSummary");
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToShareCodeConfirmation_WhenSectionTitleIsDeclarationInformation()
    {
        var checkYourAnswerQuestionId = Guid.NewGuid();
        _pageModel.FormSectionType = FormSectionType.Declaration;
        _pageModel.CurrentQuestionId = checkYourAnswerQuestionId;

        var formResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Declaration, Title = "Test Section" },
            Questions =
            [
                new FormQuestion
                {
                    Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
                }
            ]
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), checkYourAnswerQuestionId))
            .ReturnsAsync(new FormQuestion
            {
                Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
            });

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(formResponse);

        var result = await _pageModel.OnPostAsync();

        _formsEngineMock.Verify(
            f => f.SaveUpdateAnswers(_pageModel.FormId, _pageModel.SectionId, _pageModel.OrganisationId,
                It.IsAny<FormQuestionAnswerState>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/ShareInformation/ShareCodeConfirmation");
    }

    [Fact]
    public async Task
        OnPostAsync_ShouldRedirectToShareCodeConfirmation_WithGeneratedShareCode_WhenSectionIsDeclaration()
    {
        var shareCode = "HDJ2123F";
        _pageModel.FormSectionType = FormSectionType.Declaration;
        var currentQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var formResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Declaration, Title = "Test Section" },
            Questions =
            [
                new FormQuestion
                {
                    Id = currentQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
                }
            ]
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), currentQuestionId))
            .ReturnsAsync(new FormQuestion
            {
                Id = currentQuestionId, Type = FormQuestionType.CheckYourAnswers, NextQuestion = null
            });

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(formResponse);

        _formsEngineMock.Setup(f => f.CreateShareCodeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(shareCode);

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());

        var result = await _pageModel.OnPostAsync();

        _formsEngineMock.Verify(f => f.CreateShareCodeAsync(_pageModel.FormId, _pageModel.OrganisationId), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/ShareInformation/ShareCodeConfirmation");
        (result as RedirectToPageResult)!.RouteValues?["shareCode"].Should().Be(shareCode);
    }

    [Fact]
    public async Task OnGetAsync_RedirectsToCorrectPage_WhenPreviousUnansweredQuestionExistsAndIsNotAlternativeBranch()
    {
        var previousUnansweredQuestionId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var qPrevious = new FormQuestion
        {
            Id = previousUnansweredQuestionId, Type = FormQuestionType.Text, NextQuestion = currentQuestionId,
            Options = new FormQuestionOptions()
        };
        var qCurrent = new FormQuestion
        {
            Id = currentQuestionId, Type = FormQuestionType.Text, BranchType = FormQuestionBranchType.Main,
            NextQuestion = nextQuestionId, Options = new FormQuestionOptions()
        };
        var qNext = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, NextQuestion = null, Options = new FormQuestionOptions()
        };
        var questionsList = new List<FormQuestion> { qPrevious, qCurrent, qNext };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse { Questions = questionsList });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(qCurrent);

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                questionsList,
                currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .Returns(previousUnansweredQuestionId);

        var result = await _pageModel.OnGetAsync();
        _objectModelValidatorMock.Verify();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(previousUnansweredQuestionId);
        redirectResult.RouteValues!["OrganisationId"].Should().Be(_pageModel.OrganisationId);
        redirectResult.RouteValues!["FormId"].Should().Be(_pageModel.FormId);
        redirectResult.RouteValues!["SectionId"].Should().Be(_pageModel.SectionId);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToCorrectPage_WhenPreviousUnansweredQuestionExistsAndIsNotAlternativeBranch()
    {
        var previousUnansweredQuestionId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var qPrevious = new FormQuestion
        {
            Id = previousUnansweredQuestionId, Type = FormQuestionType.Text, NextQuestion = currentQuestionId,
            Options = new FormQuestionOptions()
        };
        var qCurrent = new FormQuestion
        {
            Id = currentQuestionId, Type = FormQuestionType.Text, BranchType = FormQuestionBranchType.Main,
            NextQuestion = nextQuestionId, Options = new FormQuestionOptions()
        };
        var qNext = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, NextQuestion = null, Options = new FormQuestionOptions()
        };
        var questionsList = new List<FormQuestion> { qPrevious, qCurrent, qNext };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse { Questions = questionsList });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(qCurrent);

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                questionsList,
                currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .Returns(previousUnansweredQuestionId);

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());

        var result = await _pageModel.OnPostAsync();
        _objectModelValidatorMock.Verify();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(previousUnansweredQuestionId);
        redirectResult.RouteValues!["OrganisationId"].Should().Be(_pageModel.OrganisationId);
        redirectResult.RouteValues!["FormId"].Should().Be(_pageModel.FormId);
        redirectResult.RouteValues!["SectionId"].Should().Be(_pageModel.SectionId);
    }

    [Fact]
    public async Task OnGetAsync_ReturnsPage_WhenCurrentQuestionIsAlternativeBranchAndPreviousUnansweredExists()
    {
        var previousUnansweredQuestionId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionAfterCurrentId = Guid.NewGuid();
        var mainPathQuestionId = Guid.NewGuid();
        var mainPathNextQuestionId = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var qMainPathBranching = new FormQuestion
        {
            Id = mainPathQuestionId,
            Type = FormQuestionType.YesOrNo,
            NextQuestion = mainPathNextQuestionId,
            NextQuestionAlternative = currentQuestionId,
            Options = new FormQuestionOptions()
        };
        var qMainPathNext = new FormQuestion
        {
            Id = mainPathNextQuestionId, Type = FormQuestionType.Text, NextQuestion = null,
            Options = new FormQuestionOptions()
        };
        var qCurrentAlternative = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.Text,
            BranchType = FormQuestionBranchType.Alternative,
            NextQuestion = nextQuestionAfterCurrentId,
            Options = new FormQuestionOptions()
        };
        var qNextAfterCurrent = new FormQuestion
        {
            Id = nextQuestionAfterCurrentId, Type = FormQuestionType.Text, NextQuestion = null,
            Options = new FormQuestionOptions()
        };

        var questionsList = new List<FormQuestion>
            { qMainPathBranching, qMainPathNext, qCurrentAlternative, qNextAfterCurrent };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse { Questions = questionsList });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(qCurrentAlternative);

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                questionsList,
                currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .Returns(previousUnansweredQuestionId);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGetAsync_ReturnsPage_WhenNoPreviousUnansweredQuestionAndNotAlternativeBranch()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var qCurrent = new FormQuestion
        {
            Id = currentQuestionId, Type = FormQuestionType.Text, BranchType = FormQuestionBranchType.Main,
            NextQuestion = nextQuestionId, Options = new FormQuestionOptions()
        };
        var qNext = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, NextQuestion = null, Options = new FormQuestionOptions()
        };
        var questionsList = new List<FormQuestion> { qCurrent, qNext };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse { Questions = questionsList });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(qCurrent);

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                questionsList,
                currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .Returns((Guid?)null);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGetAsync_ReturnsPage_WhenPreviousUnansweredIsCurrentQuestionAndNotAlternativeBranch()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var qCurrent = new FormQuestion
        {
            Id = currentQuestionId, Type = FormQuestionType.Text, BranchType = FormQuestionBranchType.Main,
            NextQuestion = nextQuestionId, Options = new FormQuestionOptions()
        };
        var qNext = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, NextQuestion = null, Options = new FormQuestionOptions()
        };
        var questionsList = new List<FormQuestion> { qCurrent, qNext };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse { Questions = questionsList });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(qCurrent);

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                questionsList,
                currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .Returns(currentQuestionId);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToNextQuestionAlternative_WhenYesNoAnswerIsNo()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var yesNoQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Title = "Test Yes/No Question",
            Description = "Please select an option.",
            Caption = "Test Caption",
            Type = FormQuestionType.YesOrNo,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = nextQuestionAlternativeId,
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(yesNoQuestion);

        _pageModel.YesNoInputModel = new FormElementYesNoInputModel { YesNoInput = "no" };

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = false } });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null && a.Answer.BoolValue == false))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Questions =
            [
                yesNoQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() },
                new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() }
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionAlternativeId);
        redirectResult.RouteValues!["OrganisationId"].Should().Be(_pageModel.OrganisationId);
        redirectResult.RouteValues!["FormId"].Should().Be(_pageModel.FormId);
        redirectResult.RouteValues!["SectionId"].Should().Be(_pageModel.SectionId);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToNextQuestion_WhenYesNoAnswerIsYes()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var yesNoQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Title = "Test Yes/No Question",
            Description = "Please select an option.",
            Caption = "Test Caption",
            Type = FormQuestionType.YesOrNo,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = nextQuestionAlternativeId
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(yesNoQuestion);

        _pageModel.YesNoInputModel = new FormElementYesNoInputModel { YesNoInput = "yes" };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse
            {
                Questions = [yesNoQuestion]
            });

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = true } });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null && a.Answer.BoolValue == true))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                yesNoQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() },
                new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() }
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToNextQuestionAlternative_WhenFileUploadIsNoFile()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Title = "Test File Upload Question",
            Description = "Please upload a file.",
            Caption = "Test Caption",
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = nextQuestionAlternativeId,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = null };

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = null } });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId &&
                    (a.Answer == null || string.IsNullOrEmpty(a.Answer.TextValue))))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                fileUploadQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() },
                new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() }
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionAlternativeId);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToNextQuestion_WhenFileUploadIsAFile()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();
        var mockFileName = "test.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Title = "Test File Upload Question",
            Description = "Please upload a file.",
            Caption = "Test Caption",
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = nextQuestionAlternativeId,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(mockFileName);
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = mockFile.Object };

        _fileHostManagerMock.Setup(fhm => fhm.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _publisherMock.Setup(p => p.Publish(It.IsAny<ScanFile>()))
            .Returns(Task.CompletedTask);

        var orgApiIdentifier =
            new Identifier(scheme: "TestScheme", id: "TestId", legalName: "Test Legal Name", uri: null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CDP.Organisation.WebApiClient.Organisation(
                id: _pageModel.OrganisationId, name: "Test Org", identifier: orgApiIdentifier,
                additionalIdentifiers: [], addresses: [],
                contactPoint: new ContactPoint(name: "Test Contact", email: "test@example.com", telephone: null,
                    url: null),
                details: new Details(null, null, null, null, null, null, null), roles: [],
                type: OrganisationType.Organisation
            ));

        var userInfo = new UserInfo { Name = "Test User", Email = "test.user@example.com" };
        _userInfoServiceMock.Setup(uis => uis.GetUserInfo())
            .ReturnsAsync(userInfo);

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = mockFileName } });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId &&
                    (a.Answer != null && !string.IsNullOrEmpty(a.Answer.TextValue))))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                fileUploadQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() },
                new FormQuestion { Id = nextQuestionAlternativeId, Options = new FormQuestionOptions() }
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);
        _fileHostManagerMock.Verify(
            fhm => fhm.UploadFile(It.IsAny<Stream>(),
                It.Is<string>(s =>
                    s.StartsWith(Path.GetFileNameWithoutExtension(mockFileName) + "_") &&
                    s.EndsWith(Path.GetExtension(mockFileName))), "application/pdf"), Times.Once);
        _publisherMock.Verify(
            p => p.Publish(It.Is<ScanFile>(sf =>
                sf.UploadedFileName == mockFileName &&
                sf.QueueFileName.StartsWith(Path.GetFileNameWithoutExtension(mockFileName) + "_") &&
                sf.QueueFileName.EndsWith(Path.GetExtension(mockFileName)))), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_PopulatesPartialViewModel_WithExistingTextAnswer()
    {
        var currentQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;
        var existingAnswerText = "Previous answer";

        var formQuestion = new FormQuestion
            { Id = currentQuestionId, Type = FormQuestionType.Text, Options = new FormQuestionOptions() };
        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(formQuestion);

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = existingAnswerText } });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _pageModel.PartialViewName.Should().Be("_FormElementTextInput");
        _pageModel.PartialViewModel.Should().BeOfType<FormElementTextInputModel>();
        var viewModel = _pageModel.PartialViewModel as FormElementTextInputModel;
        viewModel!.TextInput.Should().Be(existingAnswerText);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToNextQuestion_ForTextQuestion()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        _pageModel.CurrentQuestionId = currentQuestionId;

        var textQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.Text,
            NextQuestion = nextQuestionId,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(textQuestion);

        _pageModel.TextInputModel = new FormElementTextInputModel { TextInput = "Some answer" };

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
        {
            QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = "Some answer" }
        });
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions = [textQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() }]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);
        redirectResult.RouteValues!["OrganisationId"].Should().Be(_pageModel.OrganisationId);
        redirectResult.RouteValues!["FormId"].Should().Be(_pageModel.FormId);
        redirectResult.RouteValues!["SectionId"].Should().Be(_pageModel.SectionId);
    }

    [Fact]
    public async Task OnPostAsync_RemovesAnswersFromAlternativeBranch_WhenYesNoAnswerChangesFromNoToYes()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var alternativeQuestionId = Guid.NewGuid();
        var questionOnAlternativePathId = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var yesNoQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.YesOrNo,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = alternativeQuestionId,
            Options = new FormQuestionOptions()
        };

        var questionOnAlternativePath = new FormQuestion
        {
            Id = questionOnAlternativePathId,
            Type = FormQuestionType.Text,
            NextQuestion = null,
            Options = new FormQuestionOptions()
        };

        var nextMainQuestion = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, Options = new FormQuestionOptions()
        };


        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(yesNoQuestion);

        var initialAnswerState = new FormQuestionAnswerState();
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = false } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = alternativeQuestionId, Answer = new FormAnswer { TextValue = "Answer on alt path" } });
        initialAnswerState.Answers.Add(new QuestionAnswer
        {
            QuestionId = questionOnAlternativePathId,
            Answer = new FormAnswer { TextValue = "Another answer on alt path" }
        });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState);

        _pageModel.YesNoInputModel = new FormElementYesNoInputModel { YesNoInput = "yes" };

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null && a.Answer.BoolValue == true))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                yesNoQuestion,
                new FormQuestion
                {
                    Id = alternativeQuestionId, Type = FormQuestionType.Text,
                    NextQuestion = questionOnAlternativePathId, Options = new FormQuestionOptions()
                },
                questionOnAlternativePath,
                nextMainQuestion
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        FormQuestionAnswerState? capturedState = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((_, state) => capturedState = state);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);

        _tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()),
            Times.Exactly(2));

        capturedState.Should().NotBeNull();
        capturedState!.Answers.Should()
            .ContainSingle(a => a.QuestionId == currentQuestionId && a.Answer!.BoolValue == true);
        capturedState!.Answers.Should().NotContain(a => a.QuestionId == alternativeQuestionId);
        capturedState!.Answers.Should().NotContain(a => a.QuestionId == questionOnAlternativePathId);
    }

    [Fact]
    public async Task OnPostAsync_RemovesAnswersFromAlternativeBranch_WhenFileUploadChangesFromNoFileToAFile()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var alternativeQuestionId = Guid.NewGuid();
        var questionOnAlternativePathId = Guid.NewGuid();
        var mockFileName = "test_file.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = alternativeQuestionId,
            Options = new FormQuestionOptions()
        };

        var questionOnAlternativePath = new FormQuestion
        {
            Id = questionOnAlternativePathId,
            Type = FormQuestionType.Text,
            NextQuestion = null,
            Options = new FormQuestionOptions()
        };

        var nextMainQuestion = new FormQuestion
        {
            Id = nextQuestionId, Type = FormQuestionType.Text, Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var initialAnswerState = new FormQuestionAnswerState();
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = null } });
        initialAnswerState.Answers.Add(new QuestionAnswer
        {
            QuestionId = alternativeQuestionId,
            Answer = new FormAnswer { TextValue = "Answer on alt path for file upload" }
        });
        initialAnswerState.Answers.Add(new QuestionAnswer
        {
            QuestionId = questionOnAlternativePathId,
            Answer = new FormAnswer { TextValue = "Another answer on alt path for file upload" }
        });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(mockFileName);
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = mockFile.Object };

        _fileHostManagerMock.Setup(fhm => fhm.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.Publish(It.IsAny<ScanFile>()))
            .Returns(Task.CompletedTask);
        var orgApiIdentifier =
            new Identifier(scheme: "TestScheme", id: "TestId", legalName: "Test Legal Name", uri: null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CDP.Organisation.WebApiClient.Organisation(
                id: _pageModel.OrganisationId, name: "Test Org", identifier: orgApiIdentifier,
                additionalIdentifiers: [], addresses: [],
                contactPoint: new ContactPoint(name: "Test Contact", email: "test@example.com", telephone: null,
                    url: null),
                details: new Details(null, null, null, null, null, null, null), roles: [],
                type: OrganisationType.Organisation
            ));
        _userInfoServiceMock.Setup(uis => uis.GetUserInfo())
            .ReturnsAsync(new UserInfo { Name = "Test User", Email = "user@example.com" });

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null &&
                    !string.IsNullOrEmpty(a.Answer.TextValue)))))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                fileUploadQuestion,
                new FormQuestion
                {
                    Id = alternativeQuestionId, Type = FormQuestionType.Text,
                    NextQuestion = questionOnAlternativePathId, Options = new FormQuestionOptions()
                },
                questionOnAlternativePath,
                nextMainQuestion
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((_, _) => { });

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);
        _fileHostManagerMock.Verify(
            fhm => fhm.UploadFile(It.IsAny<Stream>(),
                It.Is<string>(s =>
                    s.StartsWith(Path.GetFileNameWithoutExtension(mockFileName) + "_") &&
                    s.EndsWith(Path.GetExtension(mockFileName))), "application/pdf"), Times.Once);
        _publisherMock.Verify(
            p => p.Publish(It.Is<ScanFile>(sf =>
                sf.UploadedFileName == mockFileName &&
                sf.QueueFileName.StartsWith(Path.GetFileNameWithoutExtension(mockFileName) + "_") &&
                sf.QueueFileName.EndsWith(Path.GetExtension(mockFileName)))), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenNavigatingToQ4AfterNoOnQ2_ShouldRenderQ4Correctly()
    {
        var q1Id = Guid.NewGuid();
        var q2Id = Guid.NewGuid();
        var q3Id = Guid.NewGuid();
        var q4Id = Guid.NewGuid();
        var q5Id = Guid.NewGuid();

        _pageModel.CurrentQuestionId = q4Id;
        _pageModel.OrganisationId = Guid.NewGuid();
        _pageModel.FormId = Guid.NewGuid();
        _pageModel.SectionId = Guid.NewGuid();

        var q1 = new FormQuestion
            { Id = q1Id, Type = FormQuestionType.NoInput, NextQuestion = q2Id, Options = new FormQuestionOptions() };
        var q2 = new FormQuestion
        {
            Id = q2Id, Type = FormQuestionType.YesOrNo, NextQuestion = q3Id, NextQuestionAlternative = q4Id,
            Options = new FormQuestionOptions()
        };
        var q3 = new FormQuestion
            { Id = q3Id, Type = FormQuestionType.NoInput, NextQuestion = null, Options = new FormQuestionOptions() };
        var q4 = new FormQuestion
        {
            Id = q4Id, Type = FormQuestionType.NoInput, NextQuestion = q5Id, BranchType = FormQuestionBranchType.Main,
            Options = new FormQuestionOptions()
        };
        var q5 = new FormQuestion
            { Id = q5Id, Type = FormQuestionType.NoInput, NextQuestion = null, Options = new FormQuestionOptions() };

        var questionsList = new List<FormQuestion> { q1, q2, q3, q4, q5 };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse
                { Questions = questionsList, Section = new FormSection { Title = "Test Section" } });

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, q4Id))
            .ReturnsAsync(q4);

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, q3Id))
            .ReturnsAsync(q3);

        var answerState = new FormQuestionAnswerState();
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = q1Id, Answer = new FormAnswer { TextValue = "Answer Q1" } });
        answerState.Answers.Add(new QuestionAnswer
            { QuestionId = q2Id, Answer = new FormAnswer { BoolValue = false } });

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerState);

        var realFormsEngine = new FormsEngine(
            _formsApiClientMock.Object,
            _tempDataServiceMock.Object,
            _choiceProviderServiceMock.Object,
            _dataSharingClientMock.Object,
            Mock.Of<IAnswerDisplayService>()
        );

        _formsEngineMock.Setup(f => f.GetPreviousUnansweredQuestionId(
                It.Is<List<FormQuestion>>(lst => lst.SequenceEqual(questionsList)),
                q4Id,
                It.Is<FormQuestionAnswerState>(ars => ars.Equals(answerState))))
            .Returns((List<FormQuestion> qL, Guid cId, FormQuestionAnswerState aS) =>
                realFormsEngine.GetPreviousUnansweredQuestionId(qL, cId, aS));

        var result = await _pageModel.OnGetAsync();

        result.Should()
            .BeOfType<PageResult>(
                "because Q4 should be rendered, not redirected to Q3, when navigating the 'No' branch from Q2 and the FormsEngine logic is correct.");
    }

    [Fact]
    public async Task OnPostAsync_WhenYesNoChangesFromYesToNo_ShouldClearMainPathAndPreserveAlternativePathAnswers()
    {
        var currentQuestionId = Guid.NewGuid();
        var mainPathQuestionId1 = Guid.NewGuid();
        var mainPathQuestionId2 = Guid.NewGuid();
        var alternativePathQuestionId1 = Guid.NewGuid();
        var alternativePathQuestionId2 = Guid.NewGuid();

        _pageModel.CurrentQuestionId = currentQuestionId;

        var yesNoQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.YesOrNo,
            NextQuestion = mainPathQuestionId1,
            NextQuestionAlternative = alternativePathQuestionId1,
            Options = new FormQuestionOptions()
        };

        var qMain1 = new FormQuestion
        {
            Id = mainPathQuestionId1, Type = FormQuestionType.Text, NextQuestion = mainPathQuestionId2,
            Options = new FormQuestionOptions()
        };
        var qMain2 = new FormQuestion
            { Id = mainPathQuestionId2, Type = FormQuestionType.Text, Options = new FormQuestionOptions() };
        var qAlt1 = new FormQuestion
        {
            Id = alternativePathQuestionId1, Type = FormQuestionType.Text, NextQuestion = alternativePathQuestionId2,
            Options = new FormQuestionOptions()
        };
        var qAlt2 = new FormQuestion
            { Id = alternativePathQuestionId2, Type = FormQuestionType.Text, Options = new FormQuestionOptions() };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(yesNoQuestion);

        var initialAnswerState = new FormQuestionAnswerState();
        var mainPathAnswer1 = "Answer on main path 1";
        var mainPathAnswer2 = "Answer on main path 2";
        var altPathAnswer1 = "Preserved answer on alt path 1";
        var altPathAnswer2 = "Preserved answer on alt path 2";

        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = true } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = mainPathQuestionId1, Answer = new FormAnswer { TextValue = mainPathAnswer1 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = mainPathQuestionId2, Answer = new FormAnswer { TextValue = mainPathAnswer2 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = alternativePathQuestionId1, Answer = new FormAnswer { TextValue = altPathAnswer1 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = alternativePathQuestionId2, Answer = new FormAnswer { TextValue = altPathAnswer2 } });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState);

        _pageModel.YesNoInputModel = new FormElementYesNoInputModel { YesNoInput = "no" };

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null && a.Answer.BoolValue == false))))
            .ReturnsAsync(qAlt1);

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions = [yesNoQuestion, qMain1, qMain2, qAlt1, qAlt2]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        FormQuestionAnswerState? capturedStateAfterChanges = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((key, state) =>
            {
                if (key.Contains("_Answers"))
                {
                    capturedStateAfterChanges = new FormQuestionAnswerState
                        { AnswerSetId = state.AnswerSetId, Answers = new List<QuestionAnswer>(state.Answers) };
                }
            });

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(qAlt1.Id);

        _tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()),
            Times.AtLeastOnce());

        capturedStateAfterChanges.Should().NotBeNull();
        capturedStateAfterChanges!.Answers.Should()
            .ContainSingle(a => a.QuestionId == currentQuestionId && a.Answer!.BoolValue == false);
        capturedStateAfterChanges!.Answers.Should().NotContain(a => a.QuestionId == mainPathQuestionId1);
        capturedStateAfterChanges!.Answers.Should().NotContain(a => a.QuestionId == mainPathQuestionId2);
        capturedStateAfterChanges!.Answers.Should().ContainSingle(a =>
            a.QuestionId == alternativePathQuestionId1 && a.Answer!.TextValue == altPathAnswer1);
        capturedStateAfterChanges!.Answers.Should().ContainSingle(a =>
            a.QuestionId == alternativePathQuestionId2 && a.Answer!.TextValue == altPathAnswer2);
    }

    [Fact]
    public async Task
        OnPostAsync_WhenFileUploadChangesFromNoFileToFile_ShouldClearAlternativePathAndPreserveMainPathAnswers()
    {
        var currentQuestionId = Guid.NewGuid();
        var mainPathQuestionId1 = Guid.NewGuid();
        var mainPathQuestionId2 = Guid.NewGuid();
        var alternativePathQuestionId1 = Guid.NewGuid();
        var alternativePathQuestionId2 = Guid.NewGuid();
        var newFileName = "new_test_file.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.FileUpload,
            NextQuestion = mainPathQuestionId1,
            NextQuestionAlternative = alternativePathQuestionId1,
            Options = new FormQuestionOptions()
        };

        var qMain1 = new FormQuestion
        {
            Id = mainPathQuestionId1, Type = FormQuestionType.Text, NextQuestion = mainPathQuestionId2,
            Options = new FormQuestionOptions()
        };
        var qMain2 = new FormQuestion
            { Id = mainPathQuestionId2, Type = FormQuestionType.Text, Options = new FormQuestionOptions() };
        var qAlt1 = new FormQuestion
        {
            Id = alternativePathQuestionId1, Type = FormQuestionType.Text, NextQuestion = alternativePathQuestionId2,
            Options = new FormQuestionOptions()
        };
        var qAlt2 = new FormQuestion
            { Id = alternativePathQuestionId2, Type = FormQuestionType.Text, Options = new FormQuestionOptions() };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var initialAnswerState = new FormQuestionAnswerState();
        var mainPathAnswer1 = "Preserved answer on main path 1";
        var mainPathAnswer2 = "Preserved answer on main path 2";
        var altPathAnswer1 = "Answer on alt path 1";
        var altPathAnswer2 = "Answer on alt path 2";

        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = null } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = mainPathQuestionId1, Answer = new FormAnswer { TextValue = mainPathAnswer1 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = mainPathQuestionId2, Answer = new FormAnswer { TextValue = mainPathAnswer2 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = alternativePathQuestionId1, Answer = new FormAnswer { TextValue = altPathAnswer1 } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = alternativePathQuestionId2, Answer = new FormAnswer { TextValue = altPathAnswer2 } });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(newFileName);
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = mockFile.Object };

        _fileHostManagerMock.Setup(fhm => fhm.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.Publish(It.IsAny<ScanFile>()))
            .Returns(Task.CompletedTask);
        var orgApiIdentifier =
            new Identifier(scheme: "TestScheme", id: "TestId", legalName: "Test Legal Name", uri: null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CDP.Organisation.WebApiClient.Organisation(
                id: _pageModel.OrganisationId, name: "Test Org", identifier: orgApiIdentifier,
                additionalIdentifiers: [], addresses: [],
                contactPoint: new ContactPoint(name: "Test Contact", email: "test@example.com", telephone: null,
                    url: null),
                details: new Details(null, null, null, null, null, null, null), roles: [],
                type: OrganisationType.Organisation
            ));
        _userInfoServiceMock.Setup(uis => uis.GetUserInfo())
            .ReturnsAsync(new UserInfo { Name = "Test User", Email = "user@example.com" });


        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.Is<FormQuestionAnswerState>(s => s.Answers.Any(a =>
                    a.QuestionId == currentQuestionId && a.Answer != null &&
                    !string.IsNullOrEmpty(a.Answer.TextValue)))))
            .ReturnsAsync(qMain1);

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions = [fileUploadQuestion, qMain1, qMain2, qAlt1, qAlt2]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        FormQuestionAnswerState? capturedStateAfterChanges = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((key, state) =>
            {
                if (key.Contains("_Answers"))
                {
                    capturedStateAfterChanges = new FormQuestionAnswerState
                        { AnswerSetId = state.AnswerSetId, Answers = new List<QuestionAnswer>(state.Answers) };
                }
            });

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(mainPathQuestionId1);

        _tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()),
            Times.AtLeastOnce());

        capturedStateAfterChanges.Should().NotBeNull();
        capturedStateAfterChanges!.Answers.Should().ContainSingle(a =>
            a.QuestionId == currentQuestionId && a.Answer!.TextValue != null &&
            a.Answer.TextValue.Contains(Path.GetFileNameWithoutExtension(newFileName)));
        capturedStateAfterChanges!.Answers.Should().NotContain(a => a.QuestionId == alternativePathQuestionId1);
        capturedStateAfterChanges!.Answers.Should().NotContain(a => a.QuestionId == alternativePathQuestionId2);
        capturedStateAfterChanges!.Answers.Should().ContainSingle(a =>
            a.QuestionId == mainPathQuestionId1 && a.Answer!.TextValue == mainPathAnswer1);
        capturedStateAfterChanges!.Answers.Should().ContainSingle(a =>
            a.QuestionId == mainPathQuestionId2 && a.Answer!.TextValue == mainPathAnswer2);

        _fileHostManagerMock.Verify(
            fhm => fhm.UploadFile(It.IsAny<Stream>(),
                It.Is<string>(s =>
                    s.StartsWith(Path.GetFileNameWithoutExtension(newFileName) + "_") &&
                    s.EndsWith(Path.GetExtension(newFileName))), "application/pdf"), Times.Once);
        _publisherMock.Verify(
            p => p.Publish(It.Is<ScanFile>(sf =>
                sf.UploadedFileName == newFileName &&
                sf.QueueFileName.StartsWith(Path.GetFileNameWithoutExtension(newFileName) + "_") &&
                sf.QueueFileName.EndsWith(Path.GetExtension(newFileName)))), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenTopLevelYesNoAnswerChanges_ClearsAnswersFromMultiLevelPreviouslyTakenBranch()
    {
        var q1Id = Guid.NewGuid();
        var q2YId = Guid.NewGuid();
        var q3YyId = Guid.NewGuid();
        var q4YyyId = Guid.NewGuid();
        var q4YynId = Guid.NewGuid();
        var q3YnId = Guid.NewGuid();

        var q2NId = Guid.NewGuid();
        var q3NyId = Guid.NewGuid();
        var q3NnId = Guid.NewGuid();

        var q1 = new FormQuestion
        {
            Id = q1Id, Type = FormQuestionType.YesOrNo, NextQuestion = q2YId, NextQuestionAlternative = q2NId,
            Options = new FormQuestionOptions()
        };
        var q2Y = new FormQuestion
        {
            Id = q2YId, Type = FormQuestionType.YesOrNo, NextQuestion = q3YyId, NextQuestionAlternative = q3YnId,
            Options = new FormQuestionOptions()
        };
        var q3Yy = new FormQuestion
        {
            Id = q3YyId, Type = FormQuestionType.YesOrNo, NextQuestion = q4YyyId, NextQuestionAlternative = q4YynId,
            Options = new FormQuestionOptions()
        };
        var q4Yyy = new FormQuestion
            { Id = q4YyyId, Type = FormQuestionType.Text, Options = new FormQuestionOptions(), Title = "Q4YYY Text" };
        var q4Yyn = new FormQuestion
            { Id = q4YynId, Type = FormQuestionType.Text, Options = new FormQuestionOptions(), Title = "Q4YYN Text" };
        var q3Yn = new FormQuestion
            { Id = q3YnId, Type = FormQuestionType.Text, Options = new FormQuestionOptions(), Title = "Q3YN Text" };

        var q2N = new FormQuestion
        {
            Id = q2NId, Type = FormQuestionType.YesOrNo, NextQuestion = q3NyId, NextQuestionAlternative = q3NnId,
            Options = new FormQuestionOptions()
        };
        var q3Ny = new FormQuestion
            { Id = q3NyId, Type = FormQuestionType.Text, Options = new FormQuestionOptions(), Title = "Q3NY Text" };
        var q3Nn = new FormQuestion
            { Id = q3NnId, Type = FormQuestionType.Text, Options = new FormQuestionOptions(), Title = "Q3NN Text" };

        var allQuestions = new List<FormQuestion> { q1, q2Y, q3Yy, q4Yyy, q4Yyn, q3Yn, q2N, q3Ny, q3Nn };

        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(new SectionQuestionsResponse
            {
                Questions = allQuestions,
                Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" }
            });

        var initialAnswerState = new FormQuestionAnswerState();
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = q1Id, Answer = new FormAnswer { BoolValue = true } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = q2YId, Answer = new FormAnswer { BoolValue = true } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = q3YyId, Answer = new FormAnswer { BoolValue = true } });
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = q4YyyId, Answer = new FormAnswer { TextValue = "Answer for Q4YYY" } });

        var unrelatedAnswerText = "Unrelated Answer for Q3NY";
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = q3NyId, Answer = new FormAnswer { TextValue = unrelatedAnswerText } });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(new FormQuestionAnswerState { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(new FormQuestionAnswerState { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(new FormQuestionAnswerState { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) });


        FormQuestionAnswerState? capturedState = null;
        _tempDataServiceMock.Setup(t =>
                t.Put(It.Is<string>(s => s.Contains("_Answers")), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((key, state) =>
            {
                capturedState = new FormQuestionAnswerState
                    { AnswerSetId = state.AnswerSetId, Answers = new List<QuestionAnswer>(state.Answers) };

                _tempDataServiceMock
                    .Setup(t2 => t2.PeekOrDefault<FormQuestionAnswerState>(It.Is<string>(s => s == key)))
                    .Returns(new FormQuestionAnswerState
                        { AnswerSetId = state.AnswerSetId, Answers = new List<QuestionAnswer>(state.Answers) });
            });

        _pageModel.CurrentQuestionId = q1Id;
        _pageModel.YesNoInputModel = new FormElementYesNoInputModel { YesNoInput = "no" };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, q1Id))
            .ReturnsAsync(q1);

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, q1Id,
                It.Is<FormQuestionAnswerState>(s =>
                    s.Answers.Any(a => a.QuestionId == q1Id && a.Answer != null && a.Answer.BoolValue == false))))
            .ReturnsAsync(q2N);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(q2NId);

        capturedState.Should().NotBeNull();
        capturedState!.Answers.Should().ContainSingle(a => a.QuestionId == q1Id && a.Answer!.BoolValue == false,
            "Q1 answer should be updated to No");

        capturedState.Answers.Should().NotContain(a => a.QuestionId == q2YId,
            "Answer for Q2Y (previous Yes branch) should be cleared");
        capturedState.Answers.Should().NotContain(a => a.QuestionId == q3YyId,
            "Answer for Q3YY (previous Yes branch) should be cleared");
        capturedState.Answers.Should().NotContain(a => a.QuestionId == q4YyyId,
            "Answer for Q4YYY (previous Yes branch) should be cleared");

        capturedState.Answers.Should().ContainSingle(
            a => a.QuestionId == q3NyId && a.Answer!.TextValue == unrelatedAnswerText,
            "Unrelated answer on the Q1=No path (Q3NY) should be preserved");

        _tempDataServiceMock.Verify(
            t => t.Put(It.Is<string>(s => s.Contains("_Answers")), It.IsAny<FormQuestionAnswerState>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task OnPostAsync_WhenFileUploadQuestionAndNoNewFileUploaded_ShouldRetainExistingFileAnswer()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var originalFileName = "original_test_file.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var initialAnswerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                    { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = originalFileName } }
            }
        };

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(() => new FormQuestionAnswerState
                { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(() => new FormQuestionAnswerState
                { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(() => new FormQuestionAnswerState
                { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(() => new FormQuestionAnswerState
                { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) })
            .Returns(() => new FormQuestionAnswerState
                { Answers = new List<QuestionAnswer>(initialAnswerState.Answers) });

        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = null, UploadedFileName = originalFileName, HasValue = true };

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                fileUploadQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() }
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        FormQuestionAnswerState? capturedState = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((_, state) => capturedState = state);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);

        _tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()), Times.Once);
        capturedState.Should().NotBeNull();
        var answerInState = capturedState!.Answers.FirstOrDefault(a => a.QuestionId == currentQuestionId);
        answerInState.Should().NotBeNull();
        answerInState!.Answer.Should().NotBeNull();
        answerInState.Answer!.TextValue.Should().Be(originalFileName, "the original file name should be retained");

        _fileHostManagerMock.Verify(
            fhm => fhm.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<ScanFile>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenReplacingAnExistingFile_ShouldUpdateAnswerAndPreservePath()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var questionOnMainPathId = Guid.NewGuid();
        var originalFileName = "original_file.pdf";
        var newFileName = "new_file.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            Options = new FormQuestionOptions()
        };

        var questionOnMainPath = new FormQuestion
        {
            Id = questionOnMainPathId,
            Type = FormQuestionType.Text,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId,
                    currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var initialAnswerState = new FormQuestionAnswerState();
        initialAnswerState.Answers.Add(new QuestionAnswer
            { QuestionId = currentQuestionId, Answer = new FormAnswer { TextValue = originalFileName } });
        initialAnswerState.Answers.Add(new QuestionAnswer
        {
            QuestionId = questionOnMainPathId, Answer = new FormAnswer { TextValue = "Existing answer on main path" }
        });

        _tempDataServiceMock.SetupSequence(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState)
            .Returns(initialAnswerState);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(newFileName);
        mockFile.Setup(f => f.Length).Returns(2048);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        _pageModel.FileUploadModel = new FormElementFileUploadModel { UploadedFile = mockFile.Object };

        var orgApiIdentifier =
            new Identifier(scheme: "TestScheme", id: "TestId", legalName: "Test Legal Name", uri: null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CDP.Organisation.WebApiClient.Organisation(
                id: _pageModel.OrganisationId, name: "Test Org", identifier: orgApiIdentifier,
                additionalIdentifiers: [], addresses: [],
                contactPoint: new ContactPoint(name: "Test Contact", email: "test@example.com", telephone: null,
                    url: null),
                details: new Details(null, null, null, null, null, null, null), roles: [],
                type: OrganisationType.Organisation
            ));
        _userInfoServiceMock.Setup(uis => uis.GetUserInfo())
            .ReturnsAsync(new UserInfo { Name = "Test User", Email = "user@example.com" });

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .ReturnsAsync(new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() });

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions =
            [
                fileUploadQuestion, new FormQuestion { Id = nextQuestionId, Options = new FormQuestionOptions() },
                questionOnMainPath
            ]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        FormQuestionAnswerState? capturedState = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((_, state) => capturedState = state);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)!.RouteValues!["CurrentQuestionId"].Should().Be(nextQuestionId);

        capturedState.Should().NotBeNull();
        var fileAnswer = capturedState!.Answers.FirstOrDefault(a => a.QuestionId == currentQuestionId);
        fileAnswer.Should().NotBeNull();
        fileAnswer!.Answer!.TextValue.Should().NotBe(originalFileName);
        fileAnswer.Answer.TextValue.Should().Contain(Path.GetFileNameWithoutExtension(newFileName));

        var preservedAnswer = capturedState.Answers.FirstOrDefault(a => a.QuestionId == questionOnMainPathId);
        preservedAnswer.Should().NotBeNull();
        preservedAnswer!.Answer!.TextValue.Should().Be("Existing answer on main path");

        _fileHostManagerMock.Verify(
            fhm => fhm.UploadFile(It.IsAny<Stream>(),
                It.Is<string>(s => s.Contains(Path.GetFileNameWithoutExtension(newFileName))), "application/pdf"),
            Times.Once);
        _publisherMock.Verify(p => p.Publish(It.Is<ScanFile>(sf => sf.UploadedFileName == newFileName)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenOptionalFileUploadAnswerIsChangedToNo_ShouldClearAnswerAndRedirectToAlternative()
    {
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var alternativeQuestionId = Guid.NewGuid();
        var originalFileName = "existing_file.pdf";

        _pageModel.CurrentQuestionId = currentQuestionId;

        var fileUploadQuestion = new FormQuestion
        {
            Id = currentQuestionId,
            Type = FormQuestionType.FileUpload,
            NextQuestion = nextQuestionId,
            NextQuestionAlternative = alternativeQuestionId,
            Options = new FormQuestionOptions()
        };

        _formsEngineMock.Setup(f =>
                f.GetCurrentQuestion(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId))
            .ReturnsAsync(fileUploadQuestion);

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Standard, Title = "Test Section" },
            Questions = [fileUploadQuestion]
        };
        _formsEngineMock.Setup(f =>
                f.GetFormSectionAsync(_pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId))
            .ReturnsAsync(sectionResponse);

        var initialAnswerState = new FormQuestionAnswerState
        {
            Answers =
            [
                new()
                {
                    QuestionId = currentQuestionId,
                    Answer = new FormAnswer { TextValue = originalFileName }
                }
            ]
        };
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(initialAnswerState);

        _pageModel.FileUploadModel = new FormElementFileUploadModel
        {
            UploadedFile = null,
            UploadedFileName = originalFileName,
            HasValue = false
        };

        _formsEngineMock.Setup(f => f.GetNextQuestion(
                _pageModel.OrganisationId, _pageModel.FormId, _pageModel.SectionId, currentQuestionId,
                It.IsAny<FormQuestionAnswerState>()))
            .ReturnsAsync(new FormQuestion { Id = alternativeQuestionId, Options = new FormQuestionOptions() });

        FormQuestionAnswerState? capturedState = null;
        _tempDataServiceMock.Setup(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()))
            .Callback<string, FormQuestionAnswerState>((_, state) => capturedState = state);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues!["CurrentQuestionId"].Should().Be(alternativeQuestionId);

        _tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<FormQuestionAnswerState>()), Times.Once);
        capturedState.Should().NotBeNull();
        var answerInState = capturedState!.Answers.FirstOrDefault(a => a.QuestionId == currentQuestionId);
        answerInState.Should().NotBeNull();
        answerInState!.Answer?.TextValue.Should().BeNull("the file answer should be cleared when the user selects 'No'");
    }
}
