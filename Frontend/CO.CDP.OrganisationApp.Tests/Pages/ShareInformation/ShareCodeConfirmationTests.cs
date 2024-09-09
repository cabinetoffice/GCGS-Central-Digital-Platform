using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApiClient = CO.CDP.DataSharing.WebApiClient;


namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodeConfirmationTests
{
    private readonly Mock<WebApiClient.IDataSharingClient> _dataSharingApiClientMock;
    private readonly ShareCodeConfirmationModel _pageModel;

    public ShareCodeConfirmationTests()
    {
        _dataSharingApiClientMock = new Mock<WebApiClient.IDataSharingClient>();
        _pageModel = new ShareCodeConfirmationModel(_dataSharingApiClientMock.Object);
    }

    [Fact]
    public void ShareCodeConfirmationModel_ShouldInitializePropertiesCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var shareCode = "HDJ2123F";
        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;
        _pageModel.ShareCode = shareCode;

        _pageModel.OrganisationId.Should().Be(organisationId);
        _pageModel.FormId.Should().Be(formId);
        _pageModel.SectionId.Should().Be(sectionId);
        _pageModel.ShareCode.Should().Be(shareCode);
    }

    [Fact]
    public async Task OnGetDownload_ShouldReturnFile_WhenPdfIsDownloadedSuccessfully()
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

        _dataSharingApiClientMock
            .Setup(x => x.GetSharedDataPdfAsync(shareCode))
            .ReturnsAsync(fileResponseMock);

        _pageModel.ShareCode = shareCode;

        var result = await _pageModel.OnGetDownload();

        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult!.FileStream.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("HDJ2123F.pdf");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenShareCodeIsEmpty()
    {
        var result = await _pageModel.OnGetDownload();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenFileIsNotFound()
    {
        var shareCode = "HDJ2123F";

        _dataSharingApiClientMock
            .Setup(x => x.GetSharedDataPdfAsync(shareCode))
            .ReturnsAsync((WebApiClient.FileResponse?)null);

        var result = await _pageModel.OnGetDownload();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}