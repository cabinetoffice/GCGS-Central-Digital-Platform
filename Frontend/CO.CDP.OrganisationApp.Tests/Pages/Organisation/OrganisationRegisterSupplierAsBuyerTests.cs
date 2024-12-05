using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class OrganisationRegisterSupplierAsBuyerTests
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<ITempDataService> tempDataServiceMock;
    private readonly OrganisationRegisterSupplierAsBuyerModel _model;
    private readonly Guid orgGuid = new("8b1e9502-2dd5-49fa-ad56-26bae0f85b85");

    public OrganisationRegisterSupplierAsBuyerTests()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        tempDataServiceMock = new Mock<ITempDataService>();
        _model = new OrganisationRegisterSupplierAsBuyerModel(tempDataServiceMock.Object, organisationClientMock.Object);
    }

    [Fact]
    public async Task ShouldRedirectToSupplierToBuyerOrganisationType_WhenOrganisationIsNotABuyer()
    {
        organisationClientMock
            .Setup<Task<CDP.Organisation.WebApiClient.Organisation>>(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(OrganisationResponse(roles: new[] { PartyRole.Tenderer }, orgGuid: orgGuid));

        var initialState = new SupplierToBuyerDetails
        {
            OrganisationType = OrganisationType.Supplier
        };
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(initialState);

        var result = await _model.OnGet(orgGuid);

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(state =>
            state.OrganisationType == OrganisationType.Supplier
        )), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationType");
        redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public async Task ShouldRedirectToOrganisationOverview_WhenOrganisationIsAlreadyABuyer()
    {
        organisationClientMock
            .Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(OrganisationResponse(roles: new[] { PartyRole.Buyer }, orgGuid: orgGuid));

        var result = await _model.OnGet(orgGuid);

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.IsAny<SupplierToBuyerDetails>()), Times.Never);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationOverview");
        redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public async Task ShouldUpdateStateCorrectly_WhenOrganisationIsTenderer()
    {
        organisationClientMock
            .Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(OrganisationResponse(roles: new[] { PartyRole.Tenderer }, orgGuid: orgGuid));

        var initialState = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(initialState);

        await _model.OnGet(orgGuid);

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(state =>
            state.OrganisationType == OrganisationType.Supplier
        )), Times.Once);
    }

    private static CDP.Organisation.WebApiClient.Organisation OrganisationResponse(PartyRole[]? roles = null, Guid? orgGuid = null)
    {
        return
            new CO.CDP.Organisation.WebApiClient.Organisation(
                additionalIdentifiers: null!,
                addresses: null!,
                contactPoint: null!,
                details: null!,
                id: orgGuid ?? Guid.NewGuid(),
                identifier: null!,
                name: "Org name",
                roles: roles ?? []);
    }
}