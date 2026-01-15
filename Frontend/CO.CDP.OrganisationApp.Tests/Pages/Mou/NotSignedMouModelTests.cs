using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.MoU;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Tests.TestData;

namespace CO.CDP.OrganisationApp.Tests.Pages.MoU;

public class NotSignedMouModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly NotSignedMouModel _model;

    public NotSignedMouModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        var loggerMock = new Mock<ILogger<NotSignedMouModel>>();
        _sessionMock = new Mock<ISession>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _model = new NotSignedMouModel(
            _organisationClientMock.Object,
            loggerMock.Object,
            _sessionMock.Object,
            _authorizationServiceMock.Object
        );
    }

    [Fact]
    public async Task OnGetAsync_WhenOrganisationIdIsEmpty_RedirectsToError()
    {
        _model.OrganisationId = Guid.Empty;

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenOrganisationNotFound_RedirectsToError()
    {
        var organisationId = Guid.NewGuid();
        _model.OrganisationId = organisationId;
        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation)null!);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenApiClientThrowsException_RedirectsToError()
    {
        var organisationId = Guid.NewGuid();
        _model.OrganisationId = organisationId;
        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId))
            .ThrowsAsync(new Exception("API error"));

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIsAdmin_SetsPropertiesCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(id: organisationId, name: "Test Org");
        _model.OrganisationId = organisationId;

        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId)).ReturnsAsync(organisation);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.OrganisationName.Should().Be("Test Org");
        _model.CanSignMou.Should().BeTrue();
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIsNotEditor_SetsPropertiesCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(id: organisationId, name: "Test Org");
        _model.OrganisationId = organisationId;

        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId)).ReturnsAsync(organisation);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Failed());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.OrganisationName.Should().Be("Test Org");
        _model.CanSignMou.Should().BeFalse();
    }

    [Fact]
    public async Task OnGetAsync_WhenOriginIsOrganisationHome_SetsBackLinkToOrganisationHome()
    {
        var organisationId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(id: organisationId, name: "Test Org");
        _model.OrganisationId = organisationId;
        _model.Origin = "organisation-home";

        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId)).ReturnsAsync(organisation);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.BackLinkUrl.Should().Be($"/organisation/{organisationId}/home");
    }

    [Fact]
    public async Task OnGetAsync_WhenOriginIsOverview_SetsBackLinkToOrganisationOverview()
    {
        var organisationId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(id: organisationId, name: "Test Org");
        _model.OrganisationId = organisationId;
        _model.Origin = "overview";

        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId)).ReturnsAsync(organisation);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.BackLinkUrl.Should().Be($"/organisation/{organisationId}");
    }

    [Fact]
    public async Task OnGetAsync_WhenOriginIsNull_SetsBackLinkToOrganisationOverview()
    {
        var organisationId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(id: organisationId, name: "Test Org");
        _model.OrganisationId = organisationId;
        _model.Origin = null;

        _sessionMock.Setup(s => s.Get<UserDetails>(It.IsAny<string>())).Returns(UserDetailsFactory.CreateUserDetails());
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId)).ReturnsAsync(organisation);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.BackLinkUrl.Should().Be($"/organisation/{organisationId}");
    }
}
