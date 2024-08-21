using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public async Task OnGetAsync_ShouldReturnPageResult_WhenShareCodeIsGeneratedSuccessfully()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var shareCode = "HDJ2123F";

        _pageModel.FormId = formId;
        _pageModel.OrganisationId = organisationId;
        _pageModel.SectionId = sectionId;

        _dataSharingApiClientMock
            .Setup(x => x.CreateSharedDataAsync(It.Is<WebApiClient.ShareRequest>(sr =>
                sr.FormId == formId && sr.OrganisationId == organisationId)))
            .ReturnsAsync(new WebApiClient.ShareReceipt(formId, null, shareCode));

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _pageModel.ShareCode.Should().Be(shareCode);
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToNotFound_WhenApiExceptionOccursWith404()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.FormId = formId;
        _pageModel.OrganisationId = organisationId;
        _pageModel.SectionId = sectionId;

        _dataSharingApiClientMock
            .Setup(x => x.CreateSharedDataAsync(It.IsAny<WebApiClient.ShareRequest>()))
            .ThrowsAsync(new WebApiClient.ApiException(string.Empty, 404, string.Empty, null, null));

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
        _pageModel.ShareCode.Should().BeNull();
    }

    [Fact]
    public async Task OnGetAsync_ShouldAllowMiddlewareToHandle500Error()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.FormId = formId;
        _pageModel.OrganisationId = organisationId;
        _pageModel.SectionId = sectionId;

        _dataSharingApiClientMock
            .Setup(x => x.CreateSharedDataAsync(It.IsAny<WebApiClient.ShareRequest>()))
            .ThrowsAsync(new WebApiClient.ApiException("Internal Server Error", 500, string.Empty, null, null));

        await Assert.ThrowsAsync<WebApiClient.ApiException>(async () => await _pageModel.OnGetAsync());
    }
}