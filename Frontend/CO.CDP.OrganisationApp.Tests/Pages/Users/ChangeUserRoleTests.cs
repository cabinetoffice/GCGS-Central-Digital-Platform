using Moq;
using CO.CDP.OrganisationApp.Pages.Users;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ChangeUserRoleTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ChangeUserRoleModel _changeUserRoleModel;

    public ChangeUserRoleTests()
    {
        _mockSession = new Mock<ISession>();
        _mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "whatever" });

        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _changeUserRoleModel = new ChangeUserRoleModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    // Person invite tests first, followed by person specific tests
    [Fact]
    public async Task OnGet_ShouldInitializeModel_WhenPersonInviteExists()
    {
        var mockPersonInvite = new PersonInviteModel(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(mockPersonInvite.Id))
            .ReturnsAsync(new List<PersonInviteModel>() { mockPersonInvite });

        var result = await _changeUserRoleModel.OnGetPersonInvite();

        Assert.Equal("John Smith", _changeUserRoleModel.UserFullName);
        Assert.True(_changeUserRoleModel.IsAdmin);
        Assert.Equal(PersonScopes.Editor, _changeUserRoleModel.Role);
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
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
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
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonInvitesAsync(mockPersonInvite.Id))
            .ReturnsAsync(new List<PersonInviteModel>() { mockPersonInvite });

        _changeUserRoleModel.IsAdmin = true;
        _changeUserRoleModel.Role = PersonScopes.Viewer;

        var result = await _changeUserRoleModel.OnPostPersonInvite();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectResult.PageName);

        _mockOrganisationClient.Verify(s => s.UpdatePersonInviteAsync(
                                                It.IsAny<Guid>(),
                                                It.IsAny<Guid>(),
                                                It.Is<UpdateInvitedPersonToOrganisation>(u =>
                                                    u.Scopes.Contains(PersonScopes.Viewer) &&
                                                    u.Scopes.Contains(PersonScopes.Admin) &&
                                                    u.Scopes.Count == 2
                                                )
                                            ),
                                            Times.Once
                                        );
    }

    // Person specific tests below
    [Fact]
    public async Task OnGet_ShouldInitializeModel_WhenPersonExists()
    {
        var mockPerson = new Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<Organisation.WebApiClient.Person>() { mockPerson });

        var result = await _changeUserRoleModel.OnGetPerson();

        Assert.Equal("John Smith", _changeUserRoleModel.UserFullName);
        Assert.True(_changeUserRoleModel.IsAdmin);
        Assert.Equal(PersonScopes.Editor, _changeUserRoleModel.Role);
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
        var mockPerson = new Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<Organisation.WebApiClient.Person>() { mockPerson });

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
        var mockPerson = new Organisation.WebApiClient.Person(
                                    "a@b.com",
                                    "John",
                                    new Guid(),
                                    "Smith",
                                    new List<string> { PersonScopes.Admin, PersonScopes.Editor }
                                );

        _mockOrganisationClient
            .Setup(s => s.GetOrganisationPersonsAsync(mockPerson.Id))
            .ReturnsAsync(new List<Organisation.WebApiClient.Person>() { mockPerson });

        _changeUserRoleModel.IsAdmin = true;
        _changeUserRoleModel.Role = PersonScopes.Viewer;

        var result = await _changeUserRoleModel.OnPostPerson();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectResult.PageName);

        _mockOrganisationClient.Verify(s => s.UpdateOrganisationPersonAsync(
                                                It.IsAny<Guid>(),
                                                It.IsAny<Guid>(),
                                                It.Is<UpdatePersonToOrganisation>(u =>
                                                    u.Scopes.Contains(PersonScopes.Viewer) &&
                                                    u.Scopes.Contains(PersonScopes.Admin) &&
                                                    u.Scopes.Count == 2
                                                )
                                            ),
                                            Times.Once
                                        );
    }
}