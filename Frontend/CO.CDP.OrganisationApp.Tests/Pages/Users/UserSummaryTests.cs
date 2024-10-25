using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Users;
using FluentAssertions;
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
    private readonly Guid _organisationId = Guid.NewGuid();

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

        _pageModel = new UserSummaryModel(_mockOrganisationClient.Object, _mockSession.Object)
        {
            Id = _organisationId
        };
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WhenApiCallsSucceed()
    {
        var person = new CO.CDP.Organisation.WebApiClient.Person("john@johnson.com", "John", _userGuid, "Johnson", ["ADMIN"]);
        var persons = new List<CO.CDP.Organisation.WebApiClient.Person>() { person };
        var invites = new List<PersonInviteModel> { new PersonInviteModel("john@johnson.com", "John", _userGuid, "Johnson", [""]) };
        var joinRequests = new List<JoinRequestLookUp> { new JoinRequestLookUp(Guid.NewGuid(), person, OrganisationJoinRequestStatus.Pending) };

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(_organisationId))
            .ReturnsAsync(persons);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonInvitesAsync(_organisationId))
            .ReturnsAsync(invites);

        _mockOrganisationClient.Setup(c => c.GetOrganisationJoinRequestsAsync(_organisationId, null))
           .ReturnsAsync(joinRequests);

        var result = await _pageModel.OnGet(null);

        result.Should().BeOfType<PageResult>();
        _pageModel.Persons.Should().Contain(persons);
        _pageModel.PersonInvites.Should().BeEquivalentTo(invites);
        _pageModel.OrganisationJoinRequests.Should().BeEquivalentTo(joinRequests);
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenApiException404Thrown()
    {
        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(_organisationId))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _pageModel.OnGet(null);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        _pageModel.ModelState.AddModelError("HasPerson", "Required");

        var person = new CO.CDP.Organisation.WebApiClient.Person("john@johnson.com", "Johnny", _userGuid, "NoAdminJohnson", ["ADMIN"]);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(_organisationId))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>
            {
                person
            });

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_RedirectsToAddUser_WhenHasPersonIsTrue()
    {
        _pageModel.HasPerson = true;

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("add-user");
    }

    [Fact]
    public async Task OnPost_RedirectsToOrganisationPage_WhenHasPersonIsFalse()
    {
        _pageModel.HasPerson = false;

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be($"/organisation/{_organisationId}");
    }

    [Theory]
    [InlineData(OrganisationJoinRequestStatus.Accepted, "/organisation/{0}/users/{1}/change-role?handler=person")]
    [InlineData(OrganisationJoinRequestStatus.Rejected, "/organisation/{0}/users/user-summary")]
    public async Task OnGetJoinRequest_ShouldRedirectToExpectedUrl(OrganisationJoinRequestStatus decision, string expectedUrl)
    {
        var reqId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var redirectUrl = string.Format(expectedUrl, _pageModel.Id, personId);

        _mockOrganisationClient
            .Setup(client => client.UpdateOrganisationJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateJoinRequest>()))
            .Returns(Task.CompletedTask);

        var result = await _pageModel.OnGetJoinRequest(reqId, personId, decision) as RedirectResult;

        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be(redirectUrl);

        _mockOrganisationClient.Verify(client => client.UpdateOrganisationJoinRequestAsync(
            _pageModel.Id, reqId, It.Is<UpdateJoinRequest>(r => r.Status == decision)), Times.Once);
    }

    [Fact]
    public async Task OnGetJoinRequest_ShouldRedirectToPageNotFound_WhenApiExceptionIsThrown()
    {
        var reqId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _mockOrganisationClient
            .Setup(client => client.UpdateOrganisationJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateJoinRequest>()))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _pageModel.OnGetJoinRequest(reqId, personId, OrganisationJoinRequestStatus.Accepted) as RedirectResult;

        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");

        _mockOrganisationClient.Verify(client => client.UpdateOrganisationJoinRequestAsync(
            _pageModel.Id, reqId, It.IsAny<UpdateJoinRequest>()), Times.Once);
    }
}