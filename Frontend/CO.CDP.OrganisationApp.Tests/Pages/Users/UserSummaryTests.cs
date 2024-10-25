using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class UserSummaryModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _mockSession;
    private readonly UserSummaryModel _pageModel;
    private readonly UserDetails _userDetails;
    private readonly Guid _userGuid;

    public UserSummaryModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();
        _userGuid = new Guid();

        _userDetails = new UserDetails
        {
            UserUrn = null!,
            PersonId = _userGuid
        };

        _mockSession.Setup(session => session.Get<UserDetails>(It.IsAny<string>()))
            .Returns(_userDetails);
        _pageModel = new UserSummaryModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WhenApiCallsSucceed()
    {
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;

        var person = new CO.CDP.Organisation.WebApiClient.Person("john@johnson.com", "John", _userGuid, "Johnson", ["ADMIN"]);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(organisationId))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>
            {
                person
            });

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonInvitesAsync(organisationId))
            .ReturnsAsync(new List<PersonInviteModel>());

        var result = await _pageModel.OnGet(null);

        Assert.IsType<PageResult>(result);
        Assert.Contains(person, _pageModel.Persons);
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
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;
        _pageModel.ModelState.AddModelError("HasPerson", "Required");

        var person = new CO.CDP.Organisation.WebApiClient.Person("john@johnson.com", "Johnny", _userGuid, "NoAdminJohnson", ["ADMIN"]);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(organisationId))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>
            {
                person
            });

        var result = await _pageModel.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPost_RedirectsToAddUser_WhenHasPersonIsTrue()
    {
        _pageModel.HasPerson = true;
        _pageModel.Id = Guid.NewGuid();

        var result = await _pageModel.OnPost();

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("add-user", redirectResult.Url);
    }

    [Fact]
    public async Task OnPost_RedirectsToOrganisationPage_WhenHasPersonIsFalse()
    {
        _pageModel.HasPerson = false;
        var organisationId = Guid.NewGuid();
        _pageModel.Id = organisationId;

        var result = await _pageModel.OnPost();

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"/organisation/{organisationId}", redirectResult.Url);
    }
}