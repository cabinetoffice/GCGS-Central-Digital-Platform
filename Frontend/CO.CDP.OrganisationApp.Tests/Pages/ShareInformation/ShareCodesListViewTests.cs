using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.DataSharing.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodesListViewTests
{
    private readonly Mock<WebApiClient.IDataSharingClient> _dataSharingApiClientMock;
    private readonly Mock<FormsClient> _formsClientMock;
    private readonly Mock<OrganisationClient> _organisationClientMock;
    private readonly ShareCodesListViewModel _pageModel;

    public ShareCodesListViewTests()
    {
        _dataSharingApiClientMock = new Mock<WebApiClient.IDataSharingClient>();
        _formsClientMock = new Mock<FormsClient>("https://whatever", new HttpClient());
        _organisationClientMock = new Mock<OrganisationClient>("https://whatever", new HttpClient());
        _pageModel = new ShareCodesListViewModel(_dataSharingApiClientMock.Object, _organisationClientMock.Object, _formsClientMock.Object);
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

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeTrue_WhenAllSectionsAreCompleted()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallorMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(new Guid(), ConnectedEntityType.Organisation, "connected entity name", new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _pageModel.MandatorySectionsCompleted()).Should().Be(true);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenBasicInfoIsIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, false, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallorMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(new Guid(), ConnectedEntityType.Organisation, "connected entity name", new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _pageModel.MandatorySectionsCompleted()).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenConnectedEntitiesIsIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(false, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallorMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>());

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _pageModel.MandatorySectionsCompleted()).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenFormSectionsAreIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallorMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(new Guid(), ConnectedEntityType.Organisation, "connected entity name", new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 0, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _pageModel.MandatorySectionsCompleted()).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeTrue_WhenFormSectionsAreCompleteWithNoAnswers()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallorMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(new Guid(), ConnectedEntityType.Organisation, "connected entity name", new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 0, true, sectionId, "section name", FormSectionType.Standard) }));

        (await _pageModel.MandatorySectionsCompleted()).Should().Be(true);
    }
}