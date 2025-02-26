using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.MoU;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Mou;
public class MoUSummaryModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _mockSession;
    private readonly MoUSummaryModel _model;
    private static Guid _orgId = Guid.NewGuid();
    private static Guid _mouId = Guid.NewGuid();
    private static Guid _mouSignatureId = Guid.NewGuid();
    private const string _filePath = @"mou-pdfs\fts-joint-controller-agreement.pdf";

    public MoUSummaryModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();
        _model = new MoUSummaryModel(_mockOrganisationClient.Object, _mockSession.Object);
        _model.Id = _orgId;
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ShouldReturnPageResult()
    {
        var mockOrganisation = GivenOrganisationClientModel(_orgId);
        var mou = LatestMouSignature();

        _mockOrganisationClient
            .Setup(o => o.GetOrganisationAsync(_orgId))
            .ReturnsAsync(mockOrganisation);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationLatestMouSignatureAsync(_orgId))
            .ReturnsAsync(mou);
        
        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Mou.Should().NotBeNull();
    }

    [Fact]
    public async Task OnGet_WhenOrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _mockOrganisationClient.Setup(c => c.GetOrganisationAsync(_orgId))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Not Found", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }


    [Fact]
    public async Task OnPost_ShouldReturnFileStreamResult_WhenFileExists()
    {
        var mockOrganisation = GivenOrganisationClientModel(_orgId);
        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), _filePath);

        var person = new CDP.Organisation.WebApiClient.Person("test@email.com", "first_name", Guid.NewGuid(), "last_name", ["ADMIN", "SUPPORTADMIN"]);
        var mou = new CDP.Organisation.WebApiClient.Mou(DateTimeOffset.Now, "fts-joint-controller-agreement.pdf", _mouId);
        var mouSignature = new MouSignatureLatest(person, _mouSignatureId, true, "Tester", mou, "Test User", DateTimeOffset.Now);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllTextAsync(absolutePath, "Fake PDF Content");

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(mockOrganisation);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationLatestMouSignatureAsync(_orgId))
            .ReturnsAsync(mouSignature);

        var result = await _model.OnPost();

        result.Should().BeOfType<FileStreamResult>()
            .Which.ContentType.Should().Be("application/pdf");

        var fileStreamResult = result as FileStreamResult;
        fileStreamResult.Should().NotBeNull();
        fileStreamResult!.FileDownloadName.Should().Be("fts-joint-controller-agreement.pdf");
        fileStreamResult.FileStream.Should().NotBeNull();
        await fileStreamResult!.FileStream.DisposeAsync();
        File.Delete(absolutePath);
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToPageNotFound_WhenFileDoesNotExist()
    {
        var mockOrganisation = GivenOrganisationClientModel(_orgId);
        var mou = LatestMouSignature();

        _mockOrganisationClient.Setup(c => c.GetOrganisationAsync(_orgId)).ReturnsAsync(mockOrganisation);
        _mockOrganisationClient.Setup(c => c.GetOrganisationLatestMouSignatureAsync(_orgId)).ReturnsAsync(mou);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_WhenMouFileDoesNotExist_ShouldRedirectToPageNotFound()
    {
        var mockOrganisation = GivenOrganisationClientModel(_orgId);
        var mou = LatestMouSignature();

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(mockOrganisation);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationLatestMouSignatureAsync(_orgId))
            .ReturnsAsync(mou);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_WhenOrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _mockOrganisationClient.Setup(c => c.GetOrganisationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id, ICollection<PartyRole>? pendingRoles = null)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: null,
            addresses: null,
            contactPoint: null,
            id: id ?? Guid.NewGuid(),
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: pendingRoles ?? [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
           );
    }

    private static MouSignatureLatest LatestMouSignature()
    {
        var person = new CDP.Organisation.WebApiClient.Person("test@email.com", "first_name", Guid.NewGuid(), "last_name", ["ADMIN", "SUPPORTADMIN"]);
        var mou = new CDP.Organisation.WebApiClient.Mou(DateTimeOffset.Now, _filePath, _mouId);

        return new MouSignatureLatest(person, _mouSignatureId, true, "Tester", mou, "Test User", DateTimeOffset.Now);
    }
}
