using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.MoU;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Mou;
public class ReviewAndSignMemorandomTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly ReviewAndSignMemorandomModel _model;
    private readonly UserDetails _userDetails;
    private readonly Guid _userGuid;

    public ReviewAndSignMemorandomTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _userGuid = Guid.NewGuid();

        _userDetails = new UserDetails
        {
            UserUrn = null!,
            PersonId = _userGuid
        };

        _sessionMock.Setup(session => session.Get<UserDetails>(It.IsAny<string>()))
            .Returns(_userDetails);

        _model = new ReviewAndSignMemorandomModel(_organisationClientMock.Object, _sessionMock.Object);
    }

    [Fact]
    public void OnGet_ShouldInitializeWithoutErrors()
    {
        _model.OnGet();
        _model.MouLatest.Should().BeNull();
    }

    [Fact]
    public async Task OnPost_WithInvalidModelState_ShouldReturnPage()
    {
        _model.ModelState.AddModelError("Name", "Name is required.");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WithValidModelState_ShouldRedirectToCompletionPage()
    {
        var id = Guid.NewGuid();
        var mockMou = new CO.CDP.Organisation.WebApiClient.Mou(DateTimeOffset.UtcNow, @"\mou-pdfs\mou-pdf-template.pdf", Guid.NewGuid());
        var mockOrganisation = GivenOrganisationClientModel(id);

        _model.Id = id;
        _model.SignTheAgreement = true;
        _model.JobTitleValue = "Test Job Title";
        _model.Name = "Test Name";

        _organisationClientMock
            .Setup(o => o.GetLatestMouAsync())
            .ReturnsAsync(mockMou);

        _organisationClientMock
            .Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(mockOrganisation);

        _organisationClientMock
            .Setup(o => o.SignOrganisationMouAsync(It.IsAny<Guid>(), It.IsAny<SignMouRequest>()))
            .Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ReviewAndSignMemorandomComplete");

        _organisationClientMock.Verify(o => o.GetLatestMouAsync(), Times.Once);
        _organisationClientMock.Verify(o => o.SignOrganisationMouAsync(id, It.IsAny<SignMouRequest>()), Times.Once);
    }

    [Fact]
    public async Task OnGetDownload_WhenFileDoesNotExist_ShouldRedirectToPageNotFound()
    {
        var mockMou = new CO.CDP.Organisation.WebApiClient.Mou(DateTimeOffset.UtcNow, @"\mou-pdfs\mou-pdf-template.pdf", Guid.NewGuid());
        _organisationClientMock
            .Setup(c => c.GetLatestMouAsync())
            .ReturnsAsync(mockMou);

        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mou-pdfs", "mou-pdf-template.pdf");
        if (System.IO.File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        var result = await _model.OnGetDownload();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/page-not-found");
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
            details: new Details(
                approval: null,
                pendingRoles: pendingRoles ?? []
            ));
    }

}
