using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using FormAnswer = CO.CDP.Forms.WebApiClient.FormAnswer;
using FormQuestion = CO.CDP.Forms.WebApiClient.FormQuestion;
using FormQuestionType = CO.CDP.Forms.WebApiClient.FormQuestionType;
using FormSection = CO.CDP.Forms.WebApiClient.FormSection;
using FormSectionType = CO.CDP.Forms.WebApiClient.FormSectionType;
using SectionQuestionsResponse = CO.CDP.Forms.WebApiClient.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsAdditionalSummaryModelTest
{
    private readonly Mock<IFormsClient> _formsClientMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly FormsAdditionalSummaryModel _model;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _formId = Guid.NewGuid();
    private readonly Guid _sectionId = Guid.NewGuid();
    private readonly Guid _answerSetId = Guid.NewGuid();
    private readonly Guid _questionId = Guid.NewGuid();

    public FormsAdditionalSummaryModelTest()
    {
        var evaluators = new List<IEvaluator> { new StringFormatEvaluator() };
        var evaluatorFactoryMock = new EvaluatorFactory(evaluators);
        var choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _formsClientMock = new Mock<IFormsClient>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _formsEngineMock = new Mock<IFormsEngine>();

        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        _model = new FormsAdditionalSummaryModel(
            _formsClientMock.Object,
            choiceProviderServiceMock.Object,
            evaluatorFactoryMock,
            _authorizationServiceMock.Object,
            _tempDataServiceMock.Object,
            _formsEngineMock.Object)
        {
            OrganisationId = _organisationId,
            FormId = _formId,
            SectionId = _sectionId
        };
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenFormSectionIsAdditional()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SectionTitle.Should().Be("Test Section");
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenFormSectionIsWelshAdditional()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.WelshAdditionalSection, withAnswerSets: true));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SectionTitle.Should().Be("Test Section");
    }

    [Fact]
    public async Task OnGet_ShouldReturnPageWithEmptyAnswers_WhenNoAnswerSetsExist()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: false));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SectionTitle.Should().Be("Test Section");
        _model.AnswerSummaries.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToFormsAnswerSetSummary_WhenFormSectionIsNotAdditional()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.Standard, withAnswerSets: true));

        var result = await _model.OnGet();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsAnswerSetSummary");
        redirectToPageResult.RouteValues.Should().ContainKey("OrganisationId").WhoseValue.Should().Be(_organisationId);
        redirectToPageResult.RouteValues.Should().ContainKey("FormId").WhoseValue.Should().Be(_formId);
        redirectToPageResult.RouteValues.Should().ContainKey("SectionId").WhoseValue.Should().Be(_sectionId);
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenFormsClientThrows404()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ThrowsAsync(new ApiException("Not found", 404, "", default, null));

        var result = await _model.OnGet();

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldUseFormatterStrategy_WhenSummaryRenderFormatterExists()
    {
        var summaryRenderFormatter = new SummaryRenderFormatter(
            keyExpression: "key",
            keyParams: [],
            keyExpressionOperation: ExpressionOperationType.StringFormat,
            valueExpression: "value",
            valueParams: [],
            valueExpressionOperation: ExpressionOperationType.StringFormat
        );

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true, summaryRenderFormatter));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldUseDefaultStrategy_WhenSummaryRenderFormatterIsNull()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToCheckYourAnswersQuestion_WhenUserIsAuthorizedAndAnswerSetsExistWithCheckYourAnswersQuestion()
    {
        // Arrange
        var checkYourAnswersQuestionId = Guid.NewGuid();
        var questions = new List<FormQuestion>
        {
            new FormQuestion(
                id: checkYourAnswersQuestionId,
                nextQuestion: null,
                nextQuestionAlternative: null,
                type: FormQuestionType.CheckYourAnswers,
                isRequired: false,
                title: "Check your answers",
                description: null,
                caption: null,
                summaryTitle: null,
                options: null,
                name: "check_your_answers")
        };

        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true, questions: questions));

        // Act
        var result = await _model.OnGet();

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsQuestionPage");
        redirectToPageResult.RouteValues.Should().ContainKey("OrganisationId").WhoseValue.Should().Be(_organisationId);
        redirectToPageResult.RouteValues.Should().ContainKey("FormId").WhoseValue.Should().Be(_formId);
        redirectToPageResult.RouteValues.Should().ContainKey("SectionId").WhoseValue.Should().Be(_sectionId);
        redirectToPageResult.RouteValues.Should().ContainKey("CurrentQuestionId").WhoseValue.Should().Be(checkYourAnswersQuestionId);

        // Verify temp data was set
        _tempDataServiceMock.Verify(x => x.Put(
            $"Form_{_organisationId}_{_formId}_{_sectionId}_Answers",
            It.IsAny<FormQuestionAnswerState>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToFirstQuestion_WhenUserIsAuthorizedAndNoAnswerSetsExist()
    {
        // Arrange
        var currentQuestion = new CO.CDP.OrganisationApp.Models.FormQuestion
        {
            Id = _questionId,
            Title = "First Question"
        };

        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: false));

        _formsEngineMock.Setup(x => x.GetCurrentQuestion(_organisationId, _formId, _sectionId, null))
            .ReturnsAsync(currentQuestion);

        // Act
        var result = await _model.OnGet();

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsQuestionPage");
        redirectToPageResult.RouteValues.Should().ContainKey("OrganisationId").WhoseValue.Should().Be(_organisationId);
        redirectToPageResult.RouteValues.Should().ContainKey("FormId").WhoseValue.Should().Be(_formId);
        redirectToPageResult.RouteValues.Should().ContainKey("SectionId").WhoseValue.Should().Be(_sectionId);
        redirectToPageResult.RouteValues.Should().ContainKey("CurrentQuestionId").WhoseValue.Should().Be(_questionId);
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenUserIsAuthorizedButFormsEngineReturnsNull()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: false));

        _formsEngineMock.Setup(x => x.GetCurrentQuestion(_organisationId, _formId, _sectionId, null))
            .ReturnsAsync((CO.CDP.OrganisationApp.Models.FormQuestion?)null);

        // Act
        var result = await _model.OnGet();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToFirstQuestionPage_WhenUserIsAuthorizedAndAnswerSetsExistButNoCheckYourAnswersQuestion()
    {
        // Arrange
        var questions = new List<FormQuestion>
        {
            new FormQuestion(
                id: _questionId,
                nextQuestion: null,
                nextQuestionAlternative: null,
                type: FormQuestionType.Text,
                isRequired: false,
                title: "Regular Question",
                description: null,
                caption: null,
                summaryTitle: null,
                options: null,
                name: "regular_question")
        };

        var currentQuestion = new CO.CDP.OrganisationApp.Models.FormQuestion
        {
            Id = _questionId,
            Title = "Regular Question"
        };

        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true, questions: questions));

        _formsEngineMock.Setup(x => x.GetCurrentQuestion(_organisationId, _formId, _sectionId, null))
            .ReturnsAsync(currentQuestion);

        // Act
        var result = await _model.OnGet();

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsQuestionPage");
        redirectToPageResult.RouteValues.Should().ContainKey("CurrentQuestionId").WhoseValue.Should().Be(_questionId);

        // Verify temp data was set
        _tempDataServiceMock.Verify(x => x.Put(
            $"Form_{_organisationId}_{_formId}_{_sectionId}_Answers",
            It.IsAny<FormQuestionAnswerState>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_ShouldNotRedirect_WhenUserIsNotAuthorized()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Failed());

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateMockSectionQuestionsResponse(FormSectionType.AdditionalSection, withAnswerSets: true));

        // Act
        var result = await _model.OnGet();

        // Assert
        result.Should().BeOfType<PageResult>();
        _model.SectionTitle.Should().Be("Test Section");

        // Verify no temp data was set and forms engine was not called
        _tempDataServiceMock.Verify(x => x.Put(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _formsEngineMock.Verify(x => x.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Never);
    }

    private FormAnswerSet CreateMockAnswerSet()
    {
        var answers = new List<FormAnswer>
        {
            new(id: Guid.NewGuid(), questionId: _questionId, textValue: "Test Answer", boolValue: null,
                dateValue: null, numericValue: null, optionValue: null, startValue: null, endValue: null,
                addressValue: null, jsonValue: null)
        };

        return new FormAnswerSet(id: _answerSetId, answers: answers, furtherQuestionsExempted: false);
    }

    private SectionQuestionsResponse CreateMockSectionQuestionsResponse(
        FormSectionType sectionType,
        bool withAnswerSets,
        SummaryRenderFormatter? summaryRenderFormatter = null,
        List<FormQuestion>? questions = null)
    {
        var answerSets = withAnswerSets
            ? new List<FormAnswerSet> { CreateMockAnswerSet() }
            : new List<FormAnswerSet>();

        questions ??= new List<FormQuestion>();

        return new SectionQuestionsResponse(
            section: new FormSection(
                title: "Test Section",
                type: sectionType,
                allowsMultipleAnswerSets: true,
                checkFurtherQuestionsExempted: false,
                id: _sectionId,
                configuration: new FormSectionConfiguration(
                    addAnotherAnswerLabel: null,
                    pluralSummaryHeadingFormat: null,
                    removeConfirmationCaption: "Test Caption",
                    removeConfirmationHeading: "Test Heading",
                    singularSummaryHeading: null,
                    furtherQuestionsExemptedHeading: null,
                    furtherQuestionsExemptedHint: null,
                    pluralSummaryHeadingHintFormat: null,
                    singularSummaryHeadingHint: null,
                    summaryRenderFormatter: summaryRenderFormatter
                )),
            questions: questions,
            answerSets: answerSets
        );
    }
}