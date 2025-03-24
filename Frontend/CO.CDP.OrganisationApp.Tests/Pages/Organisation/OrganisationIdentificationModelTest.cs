using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Http;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;
using WebApiClientOrganisation = CO.CDP.Organisation.WebApiClient.Organisation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationIdentificationModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly OrganisationIdentificationModel _pageModel;

    public OrganisationIdentificationModelTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        _pageModel = new OrganisationIdentificationModel(_organisationClientMock.Object, _authorizationServiceMock.Object)
        {
            Id = Guid.NewGuid(),
        };
    }

    [Fact]
    public async Task OnGet_Should_SetIsSupportAdmin_When_UserHasSupportAdminRole()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("scope", "supportadmin")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.IsSupportAdmin.Should().BeTrue();
    }


    [Fact]
    public async Task OnGet_Should_NotSetIsSupportAdmin_When_UserDoesNotHaveSupportAdminRole()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(Guid.NewGuid()));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                fakeUser,
                null,
                PersonScopeRequirement.SupportAdmin))
            .ReturnsAsync(AuthorizationResult.Failed());

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                fakeUser,
                null,
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.IsSupportAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_Should_CallSupportUpdateOrganisationAdditionalIdentifiers_When_UserIsSupportAdmin()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        _organisationClientMock
            .Setup(x => x.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<SupportUpdateOrganisation>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

        _pageModel.OrganisationScheme = new List<string> { "GB-CHC" };
        _pageModel.CharityCommissionEnglandWalesNumber = "123456";

        var result = await _pageModel.OnPost();

        _organisationClientMock.Verify(x => x.SupportUpdateOrganisationAsync(id, It.IsAny<SupportUpdateOrganisation>()), Times.Once);
        _organisationClientMock.Verify(x => x.UpdateOrganisationAsync(id, It.IsAny<UpdatedOrganisation>()), Times.Never);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPost_Should_CallUpdateOrganisationAdditionalIdentifiers_When_UserIsNotSupportAdmin()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                OrgScopeRequirement.Editor))
            .ReturnsAsync(AuthorizationResult.Success());

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                PersonScopeRequirement.SupportAdmin))
            .ReturnsAsync(AuthorizationResult.Failed());

        _organisationClientMock
            .Setup(x => x.UpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<UpdatedOrganisation>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

        _pageModel.OrganisationScheme = new List<string> { "GB-CHC" };
        _pageModel.CharityCommissionEnglandWalesNumber = "123456";

        var result = await _pageModel.OnPost();

        _organisationClientMock.Verify(x => x.SupportUpdateOrganisationAsync(id, It.IsAny<SupportUpdateOrganisation>()), Times.Never);
        _organisationClientMock.Verify(x => x.UpdateOrganisationAsync(id, It.IsAny<UpdatedOrganisation>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }


    [Fact]
    public async Task OnPost_Should_ReturnPage_When_ModelStateIsInvalid()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        _pageModel.ModelState.AddModelError("OrganisationScheme", "OrganisationScheme is required");

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
        _pageModel.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_Should_RedirectToPageNotFound_When_OrganisationIsNotFound()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WebApiClientOrganisation?)null);

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        }, "mock"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = fakeUser }
        };

        _authorizationServiceMock
            .Setup(auth => auth.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        _pageModel.OrganisationScheme = new List<string> { "GB-CHC" };

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static WebApiClientOrganisation GivenOrganisationClientModel(Guid? id)
    {
        var identifier = new Identifier("asd", "asd", "asd", new Uri("http://whatever"));
        var additionalIdentifiers = new List<Identifier>
        {
            new Identifier(
                id: "12345678",
                legalName: "Mock Legal Name 1",
                scheme: "GB-COH",
                uri: new Uri("http://example.com/1")
            )
        };

        return new WebApiClientOrganisation(
            additionalIdentifiers: additionalIdentifiers,
            addresses: null,
            contactPoint: null,
            id: id ?? Guid.NewGuid(),
            identifier: identifier,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [],
            details: new Details(
                approval: null,
                buyerInformation: null,
                pendingRoles: [],
                publicServiceMissionOrganization: null,
                scale: null,
                shelteredWorkshop: null,
                vcse: null
            )
        );
    }
}
