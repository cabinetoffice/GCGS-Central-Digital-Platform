using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsAnswerSetRemoveConfirmationModelTest
{
    private readonly Mock<IFormsClient> _formsClientMock;
    private readonly FormsAnswerSetRemoveConfirmationModel _pageModel;

    public FormsAnswerSetRemoveConfirmationModelTest()
    {
        _formsClientMock = new Mock<IFormsClient>();
        _pageModel = new FormsAnswerSetRemoveConfirmationModel(_formsClientMock.Object)
        {
            AnswerSetId = Guid.NewGuid()
        };

        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ReturnsAsync(new SectionQuestionsResponse(
             section: new FormSection(
                 title: "Test Title",
                 allowsMultipleAnswerSets: true,
                 id: Guid.NewGuid(),
                 configuration: new FormSectionConfiguration(
                     addAnotherAnswerLabel: null,
                     pluralSummaryHeadingFormat: null,
                     removeConfirmationCaption: "Test Caption",
                     removeConfirmationHeading: "Test confimration heading",
                     singularSummaryHeading: null)),
             questions: [],
             answerSets: [new FormAnswerSet(id: _pageModel.AnswerSetId, answers: [])]
             ));
    }

    [Fact]
    public async Task OnGet_InvalidPageRedirectsToNotFound()
    {
        _formsClientMock.Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ValidPageReturnsPageResult()
    {
        var result = await _pageModel.OnGet();

        _pageModel.Caption.Should().Be("Test Caption");
        _pageModel.Heading.Should().Be("Test confimration heading");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_InvalidModelStateReturnsPage()
    {
        _pageModel.ModelState.AddModelError("ConfirmRemove", "Required");

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ConfirmRemoveTrueCallsDeleteAndRedirects()
    {
        _pageModel.ConfirmRemove = true;

        var result = await _pageModel.OnPost();

        _formsClientMock.Verify(client => client.DeleteFormSectionAnswersAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("FormsAddAnotherAnswerSet");
    }

    [Fact]
    public async Task OnPost_ConfirmRemoveFalseDoesNotCallDeleteAndRedirects()
    {
        _pageModel.ConfirmRemove = false;

        var result = await _pageModel.OnPost();

        _formsClientMock.Verify(client => client.DeleteFormSectionAnswersAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("FormsAddAnotherAnswerSet");
    }
}