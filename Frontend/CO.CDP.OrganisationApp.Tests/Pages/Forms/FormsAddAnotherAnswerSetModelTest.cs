using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsAddAnotherAnswerSetModelTest
{
    private readonly Mock<IFormsClient> _formsClientMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly FormsAddAnotherAnswerSetModel _model;
    private readonly Guid AnswerSetId = Guid.NewGuid();

    public FormsAddAnotherAnswerSetModelTest()
    {
        _tempDataServiceMock = new Mock<ITempDataService>();

        _formsClientMock = new Mock<IFormsClient>();
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ReturnsAsync(new SectionQuestionsResponse(
             section: new FormSection(
                 title: "Test Title",
                 type: FormSectionType.Standard,
                 allowsMultipleAnswerSets: true,
                 id: Guid.NewGuid(),
                 configuration: new FormSectionConfiguration(
                     addAnotherAnswerLabel: null,
                     pluralSummaryHeadingFormat: null,
                     removeConfirmationCaption: "Test Caption",
                     removeConfirmationHeading: "Test confimration heading",
                     singularSummaryHeading: null)),
             questions: [],
             answerSets: [new FormAnswerSet(id: AnswerSetId, answers: [],
             furtherQuestionsExempted : false)]
             ));

        _model = new FormsAddAnotherAnswerSetModel(_formsClientMock.Object, _tempDataServiceMock.Object)
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
    public async Task OnPost_ShouldRedirectToDynamicFormsPage_WhenAddAnotherAnswerSetIsTrue()
    {
        _model.AddAnotherAnswerSet = true;

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("DynamicFormsPage");
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
    public async Task OnGetChange_ShouldRedirectToDynamicFormsPage_WhenAnswerSetIsFound()
    {
        var result = await _model.OnGetChange(AnswerSetId);

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("DynamicFormsPage");
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