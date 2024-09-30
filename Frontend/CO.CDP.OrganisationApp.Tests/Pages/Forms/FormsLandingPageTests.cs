using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsLandingPageTests
{
    private readonly Mock<IFormsClient> _mockFormsClient = new();
    private readonly Mock<IFormsEngine> _mockFormsEngine = new();
    private readonly Mock<IUserInfoService> _mockUserInfoService = new();
    private readonly Mock<ITempDataService> _mockTempDataService = new();
    private readonly FormsLandingPage _pageModel;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _formId = Guid.NewGuid();
    private readonly Guid _sectionId = Guid.NewGuid();

    public FormsLandingPageTests()
    {
        _pageModel = new FormsLandingPage(
            _mockFormsClient.Object,
            _mockFormsEngine.Object,
            _mockUserInfoService.Object,
            _mockTempDataService.Object)
        {
            OrganisationId = _organisationId,
            FormId = _formId,
            SectionId = _sectionId
        };
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToNotFound_WhenApiExceptionIsThrown()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectResult>();
        result.As<RedirectResult>().Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToSummaryPage_WhenUserIsViewerAndFormIsNotDeclaration()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId));

        _mockUserInfoService
            .Setup(x => x.UserHasScope(OrganisationPersonScopes.Viewer))
            .ReturnsAsync(true);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        result.As<RedirectToPageResult>().PageName.Should().Be("FormsAnswerSetSummary");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToSummaryPage_WhenAnswerSetAreThere()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId, furtherQuestionExmpted: false));

        _mockUserInfoService
            .Setup(x => x.UserHasScope(OrganisationPersonScopes.Viewer))
            .ReturnsAsync(false);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        result.As<RedirectToPageResult>().PageName.Should().Be("FormsAnswerSetSummary");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToCheckFurtherQuestionsExempted_WhenAnswerSetsAreExempted()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId, furtherQuestionExmpted: true));

        _mockUserInfoService
            .Setup(x => x.UserHasScope(OrganisationPersonScopes.Viewer))
            .ReturnsAsync(false);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        result.As<RedirectToPageResult>().PageName.Should().Be("FormsCheckFurtherQuestionsExempted");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToCheckFurtherQuestionsExempted_WhenNoAnswerSetsAndSectionConfigHasCheckFurtherQuestionsExempted()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId, checkFurtherQuestionsExempted: true));

        _mockUserInfoService
            .Setup(x => x.UserHasScope(OrganisationPersonScopes.Viewer))
            .ReturnsAsync(false);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        result.As<RedirectToPageResult>().PageName.Should().Be("FormsCheckFurtherQuestionsExempted");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToQuestionPage_WhenCurrentQuestionExists()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId, formSectionType: FormSectionType.Declaration));

        _mockFormsEngine
            .Setup(x => x.GetCurrentQuestion(_organisationId, _formId, _sectionId, null))
            .ReturnsAsync(new Models.FormQuestion());

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        result.As<RedirectToPageResult>().PageName.Should().Be("FormsQuestionPage");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToNotFound_WhenCurrentQuestionIsNull()
    {
        _mockFormsClient
            .Setup(x => x.GetFormSectionQuestionsAsync(_formId, _sectionId, _organisationId))
            .ReturnsAsync(CreateApiSectionQuestionsResponse(_sectionId, formSectionType: FormSectionType.Declaration));

        _mockFormsEngine
            .Setup(x => x.GetCurrentQuestion(_organisationId, _formId, _sectionId, null))
            .ReturnsAsync((Models.FormQuestion?)null);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectResult>();
        result.As<RedirectResult>().Url.Should().Be("/page-not-found");
    }

    private static SectionQuestionsResponse CreateApiSectionQuestionsResponse(
        Guid sectionId, bool? furtherQuestionExmpted = null,
        FormSectionType? formSectionType = null, bool? checkFurtherQuestionsExempted = null)
    {
        var response = new SectionQuestionsResponse(
            section: new FormSection(
                type: formSectionType ?? FormSectionType.Standard,
                allowsMultipleAnswerSets: true,
                checkFurtherQuestionsExempted: checkFurtherQuestionsExempted ?? false,
                configuration: new FormSectionConfiguration(
                    singularSummaryHeading: null,
                    pluralSummaryHeadingFormat: null,
                    addAnotherAnswerLabel: null,
                    removeConfirmationCaption: null,
                    removeConfirmationHeading: null,
                    furtherQuestionsExemptedHeading: null
                ),
                id: sectionId,
                title: "SectionTitle"
            ),
            questions:
            [
                new FormQuestion(
                    id:  Guid.NewGuid(),
                    title: "Question1",
                    description: "Description1",
                    caption: "Caption1",
                    summaryTitle: "Question1 Title",
                    type: FormQuestionType.Text,
                    isRequired: true,
                    nextQuestion: Guid.NewGuid(),
                    nextQuestionAlternative: null,
                    options: null
                )
            ],
            answerSets: []
        );

        if (furtherQuestionExmpted != null)
        {
            response.AnswerSets.Add(new FormAnswerSet
            (id: Guid.NewGuid(),
                furtherQuestionsExempted: furtherQuestionExmpted.Value,
                answers: []
            ));
        }

        return response;
    }
}