using Moq;
using CO.CDP.OrganisationApp.Pages.Users;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ChangePersonInviteUserRoleTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ChangeUserRoleModel _changeUserRoleModel;

    public ChangePersonInviteUserRoleTests()
    {
        _mockSession = new Mock<ISession>();
        _mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "whatever" });

        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _changeUserRoleModel = new ChangeUserRoleModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_ShouldInitializeModel_WhenPersonInviteExists()
    {
        var mockPersonInvite = new PersonInviteModel(
                                    email: "a@b.com",
                                    firstName: "John",
                                    id: new Guid(),
                                    lastName: "Smith",
                                    scopes: new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor },
                                    expiresOn: null
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(mockPersonInvite.Id))
            .ReturnsAsync(new List<PersonInviteModel>() { mockPersonInvite });

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_changeUserRoleModel.Id))
            .ReturnsAsync(OrganisationClientModel(_changeUserRoleModel.Id));

        var result = await _changeUserRoleModel.OnGetPersonInvite();

        Assert.Equal("John Smith", _changeUserRoleModel.UserFullName);
        Assert.True(_changeUserRoleModel.IsAdmin);
        Assert.Equal(OrganisationPersonScopes.Editor, _changeUserRoleModel.Role);
    }

    [Fact]
    public async Task OnGet_RedirectsToNotFound_WhenPersonInviteDoesntExist()
    {
        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("message", 404, "response", null, null));

        var result = await _changeUserRoleModel.OnGetPersonInvite();

        var redirectResult = Assert.IsType<RedirectResult>(result);

        Assert.Equal("/page-not-found", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostPersonInvite_ShouldReturnPageResult_WhenModelStateIsInvalid()
    {
        var mockPersonInvite = new PersonInviteModel(
            email: "a@b.com",
            firstName: "John",
            id: new Guid(),
            lastName: "Smith",
            scopes: new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor },
            expiresOn: null
        );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(mockPersonInvite.Id))
            .ReturnsAsync(new List<PersonInviteModel>() { mockPersonInvite });

        _changeUserRoleModel.IsAdmin = true;
        _changeUserRoleModel.Role = null;

        _changeUserRoleModel.ModelState.AddModelError("Role", "Required");

        var result = await _changeUserRoleModel.OnPostPersonInvite();
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostPersonInvite_RedirectsToNotFound_WhenPersonInviteDoesntExist()
    {
        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("message", 404, "response", null, null));

        var result = await _changeUserRoleModel.OnPostPersonInvite();

        var redirectResult = Assert.IsType<RedirectResult>(result);

        Assert.Equal("/page-not-found", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostPersonInvite_ShouldUpdatePersonInviteTableAndRedirect_WhenModelStateIsValid()
    {
        var mockPersonInvite = new PersonInviteModel(
            email: "a@b.com",
            firstName: "John",
            id: new Guid(),
            lastName: "Smith",
            scopes: new List<string> { OrganisationPersonScopes.Editor },
            expiresOn: null
        );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(mockPersonInvite.Id))
            .ReturnsAsync(new List<PersonInviteModel>() { mockPersonInvite });

        _changeUserRoleModel.Role = OrganisationPersonScopes.Viewer;

        var result = await _changeUserRoleModel.OnPostPersonInvite();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectResult.PageName);

        _mockOrganisationClient.Verify(s => s.UpdatePersonInviteAsync(
                                                It.IsAny<Guid>(),
                                                It.IsAny<Guid>(),
                                                It.Is<UpdateInvitedPersonToOrganisation>(u =>
                                                    u.Scopes.Contains(OrganisationPersonScopes.Viewer) &&
                                                    u.Scopes.Count == 1
                                                )
                                            ),
                                            Times.Once
                                        );
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