using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class UserSummaryModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly UserSummaryModel _pageModel;

    public UserSummaryModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _pageModel = new UserSummaryModel(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WhenApiCallsSucceed()
    {
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(organisationId))
            .ReturnsAsync(new List<Organisation.WebApiClient.Person>());

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonInvitesAsync(organisationId))
            .ReturnsAsync(new List<PersonInviteModel>());

        var result = await _pageModel.OnGet(null);

        Assert.IsType<PageResult>(result);
        Assert.Empty(_pageModel.Persons);
        Assert.Empty(_pageModel.PersonInvites);
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenApiException404Thrown()
    {
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(organisationId))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _pageModel.OnGet(null);

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/page-not-found", redirectResult.Url);
    }

    [Fact]
    public void OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        _pageModel.ModelState.AddModelError("HasPerson", "Required");

        var result = _pageModel.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_RedirectsToAddUser_WhenHasPersonIsTrue()
    {
        _pageModel.HasPerson = true;
        _pageModel.Id = Guid.NewGuid();

        var result = _pageModel.OnPost();

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("add-user", redirectResult.Url);
    }

    [Fact]
    public void OnPost_RedirectsToOrganisationPage_WhenHasPersonIsFalse()
    {
        _pageModel.HasPerson = false;
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;

        var result = _pageModel.OnPost();

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"/organisation/{organisationId}", redirectResult.Url);
    }
}
