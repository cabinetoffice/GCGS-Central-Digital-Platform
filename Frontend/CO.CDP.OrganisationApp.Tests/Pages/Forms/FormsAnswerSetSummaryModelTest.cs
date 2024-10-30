using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsAnswerSetSummaryModelTest
{
    private readonly Mock<IFormsClient> _formsClientMock;
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IChoiceProviderService> _choiceProviderService;
    private readonly FormsAnswerSetSummaryModel _model;
    private readonly Guid AnswerSetId = Guid.NewGuid();

    public FormsAnswerSetSummaryModelTest()
    {
        _tempDataServiceMock = new Mock<ITempDataService>();
        _choiceProviderService = new Mock<IChoiceProviderService>();
        _formsEngineMock = new();

        _formsClientMock = new Mock<IFormsClient>();
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ReturnsAsync(new SectionQuestionsResponse(
             section: new FormSection(
                 title: "Test Title",
                 type: FormSectionType.Standard,
                 allowsMultipleAnswerSets: true,
                 checkFurtherQuestionsExempted: false,
                 id: Guid.NewGuid(),
                 configuration: new FormSectionConfiguration(
                     addAnotherAnswerLabel: null,
                     pluralSummaryHeadingFormat: null,
                     removeConfirmationCaption: "Test Caption",
                     removeConfirmationHeading: "Test confimration heading",
                     singularSummaryHeading: null,
                     furtherQuestionsExemptedHeading: null,
                     furtherQuestionsExemptedHint: null
                     )),
             questions: [],
             answerSets: [new FormAnswerSet(id: AnswerSetId, answers: [],
             furtherQuestionsExempted : false)]
             ));

        _model = new FormsAnswerSetSummaryModel(_formsClientMock.Object, _formsEngineMock.Object, _tempDataServiceMock.Object, _choiceProviderService.Object)
        {
            OrganisationId = Guid.NewGuid(),
            FormId = Guid.NewGuid(),
            SectionId = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenInitAndVerifyPageReturnsTrue()
    {
        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenInitAndVerifyPageReturnsFalse()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet();

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToFormsQuestionPage_WhenAddAnotherAnswerSetIsTrue()
    {
        _model.AddAnotherAnswerSet = true;
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(new Models.FormQuestion());

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsQuestionPage");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSupplierBasicInformation_WhenAddAnotherAnswerSetIsFalse()
    {
        _model.AddAnotherAnswerSet = false;

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("../Supplier/SupplierInformationSummary");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("key", "error message");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGetChange_ShouldRedirectToFormsQuestionPage_WhenAnswerSetIsFound()
    {
        var result = await _model.OnGetChange(AnswerSetId);

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("FormsQuestionPage");
    }

    [Fact]
    public async Task OnGetChange_ShouldRedirectToPageNotFound_WhenAnswerSetIsNotFound()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGetChange(AnswerSetId);

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }
}