using Moq;
using CO.CDP.OrganisationApp.Pages.Users;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ChangePersonUserRoleTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ChangeUserRoleModel _changeUserRoleModel;

    public ChangePersonUserRoleTests()
    {
        _mockSession = new Mock<ISession>();
        _mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "whatever" });

        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _changeUserRoleModel = new ChangeUserRoleModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_ShouldInitializeModel_WhenPersonExists()
    {
        var mockPerson = new CO.CDP.Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>() { mockPerson });

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationAsync(_changeUserRoleModel.Id))
            .ReturnsAsync(OrganisationClientModel(_changeUserRoleModel.Id));

        var result = await _changeUserRoleModel.OnGetPerson();

        Assert.Equal("John Smith", _changeUserRoleModel.UserFullName);
        Assert.True(_changeUserRoleModel.IsAdmin);
        Assert.Equal(OrganisationPersonScopes.Editor, _changeUserRoleModel.Role);
    }

    [Fact]
    public async Task OnGet_RedirectsToNotFound_WhenPersonDoesntExist()
    {
        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("message", 404, "response", null, null));

        var result = await _changeUserRoleModel.OnGetPerson();

        var redirectResult = Assert.IsType<RedirectResult>(result);

        Assert.Equal("/page-not-found", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostPerson_ShouldReturnPageResult_WhenModelStateIsInvalid()
    {
        var mockPerson = new CO.CDP.Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>() { mockPerson });

        _changeUserRoleModel.IsAdmin = true;
        _changeUserRoleModel.Role = null;

        _changeUserRoleModel.ModelState.AddModelError("Role", "Required");

        var result = await _changeUserRoleModel.OnPostPerson();
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostPerson_RedirectsToNotFound_WhenPersonInviteDoesntExist()
    {
        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("message", 404, "response", null, null));

        var result = await _changeUserRoleModel.OnPostPerson();

        var redirectResult = Assert.IsType<RedirectResult>(result);

        Assert.Equal("/page-not-found", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostPerson_ShouldUpdatePersonTableAndRedirect_WhenModelStateIsValid()
    {
        var mockPerson = new CO.CDP.Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { OrganisationPersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<CO.CDP.Organisation.WebApiClient.Person>() { mockPerson });

        _changeUserRoleModel.Role = OrganisationPersonScopes.Viewer;

        var result = await _changeUserRoleModel.OnPostPerson();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectResult.PageName);

        _mockOrganisationClient.Verify(s => s.UpdateOrganisationPersonAsync(
                                                It.IsAny<Guid>(),
                                                It.IsAny<Guid>(),
                                                It.Is<UpdatePersonToOrganisation>(u =>
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