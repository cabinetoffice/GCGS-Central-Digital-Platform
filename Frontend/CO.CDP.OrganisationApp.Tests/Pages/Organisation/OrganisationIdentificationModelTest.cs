using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClientOrganisation = CO.CDP.Organisation.WebApiClient.Organisation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationIdentificationModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly OrganisationIdentificationModel _pageModel;

    public OrganisationIdentificationModelTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();

        _pageModel = new OrganisationIdentificationModel(_organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
        };
    }

    [Fact]
    public async Task OnGet_Should_ReturnPageResult_When_OrganisationIsValid()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;
        _pageModel.CharityCommissionEnglandWalesNumber = "Charity Org";
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.CharityCommissionEnglandWalesNumber.Should().Be("Charity Org");
    }

    [Fact]
    public async Task OnGet_Should_RedirectToPageNotFound_When_OrganisationIsNotFound()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WebApiClientOrganisation?)null);

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_Should_RedirectToOrganisationOverview_When_ModelStateIsValid()
    {
        var id = Guid.NewGuid();
        _pageModel.Id = id;

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        _pageModel.OrganisationScheme = new List<string> { "GB-CHC" };
        _pageModel.CharityCommissionEnglandWalesNumber = "123456";
        _pageModel.ModelState.ClearValidationState(nameof(OrganisationIdentificationModel));
        _pageModel.ModelState.MarkFieldValid(nameof(OrganisationIdentificationModel.OrganisationScheme));

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPost_Should_ReturnPage_When_ModelStateIsInvalid()
    {
        _pageModel.ModelState.AddModelError("OrganisationScheme", "OrganisationScheme is required");

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");

        _pageModel.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_Should_RedirectToPageNotFound_When_OrganisationIsNotFound()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WebApiClientOrganisation?)null);

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

        return new WebApiClientOrganisation(additionalIdentifiers: additionalIdentifiers, addresses: null, contactPoint: null, id: id ?? Guid.NewGuid(), identifier: identifier, name: "Test Org", roles: [], details: new Details(approval: null, pendingRoles: []));
    }
}
