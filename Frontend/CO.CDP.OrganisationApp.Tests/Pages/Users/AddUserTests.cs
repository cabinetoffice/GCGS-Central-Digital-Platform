using CO.CDP.Organisation.WebApiClient;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.OrganisationApp.Pages.Users;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Constants;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class AddUserModelTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly AddUserModel _addUserModel;

    public AddUserModelTests()
    {
        _mockSession = new Mock<ISession>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _addUserModel = new AddUserModel(_mockSession.Object, _mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_ShouldInitializeModel_WhenPersonInviteStateExists()
    {
        PersonInviteState? personInviteState = new PersonInviteState
        {
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            Scopes = new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
        };

        _mockSession
            .Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey))
            .Returns(personInviteState);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_addUserModel.Id))
            .ReturnsAsync(OrganisationClientModel(_addUserModel.Id));

        var result = await _addUserModel.OnGet();

        _addUserModel.FirstName.Should().Be("John");
        _addUserModel.LastName.Should().Be("Johnson");
        _addUserModel.Email.Should().Be("john@johnson.com");
        _addUserModel.IsAdmin.Should().BeTrue();
        _addUserModel.Role.Should().Be(OrganisationPersonScopes.Editor);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldNotInitializeModel_WhenPersonInviteStateDoesNotExist()
    {
        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey)).Returns((PersonInviteState)null!);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_addUserModel.Id))
            .ReturnsAsync(OrganisationClientModel(_addUserModel.Id));

        var result = await _addUserModel.OnGet();

        _addUserModel.FirstName.Should().BeNull();
        _addUserModel.LastName.Should().BeNull();
        _addUserModel.Email.Should().BeNull();
        _addUserModel.IsAdmin.Should().BeNull();
        _addUserModel.Role.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPageResult_WhenModelStateIsInvalid()
    {
        _addUserModel.ModelState.AddModelError("FirstName", "Required");

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_addUserModel.Id))
            .ReturnsAsync(OrganisationClientModel(_addUserModel.Id));

        var result = await _addUserModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldUpdateSessionAndRedirect_WhenModelStateIsValid()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";
        _addUserModel.Role = OrganisationPersonScopes.Editor;

        var initialState = new PersonInviteState();
        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey)).Returns(initialState);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationJoinRequestsAsync(_addUserModel.Id, OrganisationJoinRequestStatus.Pending))
            .ReturnsAsync([]);

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationPersonInvitesAsync(_addUserModel.Id))
            .ReturnsAsync([]);

        var result = await _addUserModel.OnPost();

        _mockSession.Verify(s => s.Set(PersonInviteState.TempDataKey, It.Is<PersonInviteState>(state =>
            state.Scopes != null &&
            state.FirstName == "John" &&
            state.LastName == "Johnson" &&
            state.Email == "john@johnson.com" &&
            state.Scopes.Contains(OrganisationPersonScopes.Editor)
        )), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("UserCheckAnswers");

        ((RedirectToPageResult)result).RouteValues?["Id"].Should().Be(_addUserModel.Id);
    }

    [Fact]
    public async Task OnPost_ShouldReturnPageResult_WhenPendingJoinRequestExists()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";
        _addUserModel.Role = OrganisationPersonScopes.Editor;

        var person = new CDP.Organisation.WebApiClient.Person(id: Guid.NewGuid(), email: _addUserModel.Email,
            scopes: [_addUserModel.Role], firstName: _addUserModel.FirstName, lastName: _addUserModel.LastName);

        var pendingJoinRequest = new JoinRequestLookUp(id: Guid.NewGuid(), person: person, OrganisationJoinRequestStatus.Pending);

        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey))
            .Returns(new PersonInviteState());

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationJoinRequestsAsync(_addUserModel.Id, OrganisationJoinRequestStatus.Pending))
            .ReturnsAsync(new List<JoinRequestLookUp> { pendingJoinRequest });

        var result = await _addUserModel.OnPost();

        result.Should().BeOfType<PageResult>();
        _addUserModel.PendingJoinRequests.Should().ContainSingle().Which.Person.Email.Should().Be("john@johnson.com");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPageResultWithError_WhenPersonInviteAlreadyExists()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";
        _addUserModel.Role = OrganisationPersonScopes.Editor;

        var existingInvite = new PersonInviteModel(
            email: "john@johnson.com",
            expiresOn: DateTimeOffset.UtcNow.AddDays(7),
            firstName: "John",
            id: Guid.NewGuid(),
            lastName: "Johnson",
            scopes: new List<string> { OrganisationPersonScopes.Editor }
        );

        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey))
            .Returns(new PersonInviteState());

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationPersonInvitesAsync(_addUserModel.Id))
            .ReturnsAsync(new List<PersonInviteModel> { existingInvite });

        var result = await _addUserModel.OnPost();

        Assert.IsType<PageResult>(result);
        Assert.True(_addUserModel.ModelState.ContainsKey("PersonInviteAlreadyExists"));
    }

    [Fact]
    public void UpdateFields_ShouldUpdateAllFields_WhenFieldsAreNotEmpty()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";

        var initialState = new PersonInviteState();
        var updatedState = _addUserModel.UpdateFields(initialState);

        updatedState.FirstName.Should().Be("John");
        updatedState.LastName.Should().Be("Johnson");
        updatedState.Email.Should().Be("john@johnson.com");
    }

    [Theory]
    [InlineData(OrganisationPersonScopes.Viewer)]
    [InlineData(OrganisationPersonScopes.Editor)]
    [InlineData(OrganisationPersonScopes.Admin)]
    public void UpdateScopes_ShouldUpdateScopes_WhenRoleIsSelected(string organisationPersonScope)
    {
        _addUserModel.Role = organisationPersonScope;

        var initialState = new PersonInviteState
        {
            Scopes = new List<string> { OrganisationPersonScopes.Viewer }
        };

        var updatedState = _addUserModel.UpdateScopes(initialState);

        updatedState.Scopes.Should().Contain(organisationPersonScope);
    }

    [Fact]
    public void InitModel_ShouldSetModelPropertiesFromState_WhenStateHasData()
    {
        var state = new PersonInviteState
        {
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            Scopes = new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
        };

        _addUserModel.InitModel(state);

        _addUserModel.FirstName.Should().Be("John");
        _addUserModel.LastName.Should().Be("Johnson");
        _addUserModel.Email.Should().Be("john@johnson.com");
        _addUserModel.IsAdmin.Should().BeTrue();
        _addUserModel.Role.Should().Be(OrganisationPersonScopes.Editor);
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            contactPoint: new ContactPoint(email: "test@test.com", name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );
}
