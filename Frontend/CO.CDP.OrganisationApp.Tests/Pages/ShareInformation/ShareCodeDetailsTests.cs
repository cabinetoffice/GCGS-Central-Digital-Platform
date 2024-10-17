using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.DataSharing.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodeDetailsTests
{
    private readonly Mock<WebApiClient.IDataSharingClient> _dataSharingApiClientMock;
    private readonly ShareCodeDetailsModel _pageModel;

    public ShareCodeDetailsTests()
    {
        _dataSharingApiClientMock = new Mock<WebApiClient.IDataSharingClient>();
        _pageModel = new ShareCodeDetailsModel(_dataSharingApiClientMock.Object);
    }
    [Fact]
    public async Task OnGet_ShouldReturnPageResult_WhenShareCodeDetailsAreRetrievedSuccessfully()
    {
        var organisationId = Guid.NewGuid();
        var shareCode = "HDJ2123F";
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;
        var questionsList = new List<WebApiClient.SharedConsentQuestionAnswer>
                {
                    new WebApiClient.SharedConsentQuestionAnswer("Answer 1",Guid.NewGuid(),"Question 1")
                };
        var sharedConsentDetails = new WebApiClient.SharedConsentDetails(
                questionsList,
                shareCode,
                DateTimeOffset.UtcNow
        );

        _dataSharingApiClientMock.Setup(x => x.GetShareCodeDetailsAsync(organisationId, shareCode))
            .ReturnsAsync(sharedConsentDetails);

        var result = await _pageModel.OnGet(shareCode);

        result.Should().BeOfType<PageResult>();
        _pageModel.SharedConsentDetails.Should().Be(sharedConsentDetails);
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenShareCodeDetailsAreNotFound()
    {
        var organisationId = Guid.NewGuid();
        var shareCode = "INVALID_CODE";

        _pageModel.OrganisationId = organisationId;

        _dataSharingApiClientMock.Setup(x => x.GetShareCodeDetailsAsync(organisationId, shareCode))
            .ThrowsAsync(new WebApiClient.ApiException("Not Found", 404, "", null, null));

        var result = await _pageModel.OnGet(shareCode);
        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldThrowException_WhenUnexpectedErrorOccurs()
    {
        var organisationId = Guid.NewGuid();
        var shareCode = "ERROR_CODE";

        _pageModel.OrganisationId = organisationId;

        _dataSharingApiClientMock.Setup(x => x.GetShareCodeDetailsAsync(organisationId, shareCode))
            .ThrowsAsync(new WebApiClient.ApiException("Internal Server Error", 500, "", null, null));

        Func<Task> act = async () => await _pageModel.OnGet(shareCode);
        await act.Should().ThrowAsync<WebApiClient.ApiException>()
            .Where(e => e.Message.Contains("Internal Server Error"));
    }

    [Fact]
    public async Task OnGetDownload_ShouldReturnFileResult_WhenPdfIsDownloadedSuccessfully()
    {
        var shareCode = "HDJ2123F";
        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
        var headers = new Dictionary<string, IEnumerable<string>>
    {
        { "Content-Disposition", new[] { "attachment; filename=HDJ2123F.pdf" } },
        { "Content-Type", new[] { "application/pdf" } }
    };
        var clientMock = new Mock<IDisposable>();
        var responseMock = new Mock<IDisposable>();
        var stream = new MemoryStream(pdfBytes);
        var fileResponseMock = new WebApiClient.FileResponse(200, headers, stream, clientMock.Object, responseMock.Object);

        _dataSharingApiClientMock.Setup(x => x.GetSharedDataFileAsync(shareCode))
            .ReturnsAsync(fileResponseMock);

        var result = await _pageModel.OnGetDownload(shareCode);

        result.Should().BeOfType<FileStreamResult>(); // Check for FileStreamResult
        var fileResult = result as FileStreamResult;
        fileResult!.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("HDJ2123F.pdf");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenPdfResponseIsNull()
    {
        var shareCode = "HDJ2123F";

        _dataSharingApiClientMock.Setup(x => x.GetSharedDataFileAsync(shareCode))
            .ReturnsAsync((WebApiClient.FileResponse?)null);

        var result = await _pageModel.OnGetDownload(shareCode);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenShareCodeIsEmpty()
    {
        var result = await _pageModel.OnGetDownload("");

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}