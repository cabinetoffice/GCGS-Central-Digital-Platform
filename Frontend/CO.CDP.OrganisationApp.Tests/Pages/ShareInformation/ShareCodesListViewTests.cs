using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.DataSharing.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodesListViewTests
{
    private readonly Mock<WebApiClient.IDataSharingClient> _dataSharingApiClientMock;
    private readonly ShareCodesListViewModel _pageModel;

    public ShareCodesListViewTests()
    {
        _dataSharingApiClientMock = new Mock<WebApiClient.IDataSharingClient>();
        _pageModel = new ShareCodesListViewModel(_dataSharingApiClientMock.Object);
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenShareCodesAreLoadedSuccessfully()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        var sharedCodes = new List<WebApiClient.SharedConsent>
        {
            new WebApiClient.SharedConsent("HDJ2123F", DateTimeOffset.UtcNow.AddDays(-2)),
            new WebApiClient.SharedConsent("ADJ2353F", DateTimeOffset.UtcNow.AddDays(-1))
        };

        _dataSharingApiClientMock
            .Setup(x => x.GetShareCodeListAsync(organisationId))
            .ReturnsAsync(sharedCodes);

        var result = await _pageModel.OnGet();
        result.Should().BeOfType<PageResult>();
        _pageModel.SharedConsentDetailsList.Should().NotBeNull();
        _pageModel.SharedConsentDetailsList.Should().HaveCount(sharedCodes.Count);
        _pageModel.SharedConsentDetailsList![0].ShareCode.Should().Be("HDJ2123F");
        _pageModel.SharedConsentDetailsList![1].ShareCode.Should().Be("ADJ2353F");
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToPageNotFound_WhenApiReturns404()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _dataSharingApiClientMock
            .Setup(x => x.GetShareCodeListAsync(organisationId))
            .ThrowsAsync(new WebApiClient.ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetAsync_ShouldHandleEmptySharedConsentDetailsList()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _dataSharingApiClientMock
            .Setup(x => x.GetShareCodeListAsync(organisationId))
            .ReturnsAsync(new List<WebApiClient.SharedConsent>());

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.SharedConsentDetailsList.Should().NotBeNull();
        _pageModel.SharedConsentDetailsList.Should().BeEmpty();
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

        var result = await _pageModel.OnGetDownload(shareCode);

        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult!.FileStream.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("HDJ2123F.pdf");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenShareCodeIsEmpty()
    {
        var result = await _pageModel.OnGetDownload("");

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetDownload_ShouldRedirectToPageNotFound_WhenFileIsNotFound()
    {
        var shareCode = "HDJ2123F";

        _dataSharingApiClientMock
            .Setup(x => x.GetSharedDataPdfAsync(shareCode))
            .ReturnsAsync((WebApiClient.FileResponse?)null);

        var result = await _pageModel.OnGetDownload(shareCode);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}