using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class OrganisationRegisterBuyerAsSupplierTests
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<ITempDataService> tempDataService;
    private readonly OrganisationRegisterBuyerAsSupplierModel _model;
    private readonly Guid orgGuid = new("8b1e9502-2dd5-49fa-ad56-26bae0f85b85");

    public OrganisationRegisterBuyerAsSupplierTests()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        tempDataService = new Mock<ITempDataService>();
        _model = new OrganisationRegisterBuyerAsSupplierModel(organisationClientMock.Object, tempDataService.Object);
    }

    [Fact]
    public async Task ShouldUpdateOrganisation_WhenOrganisationIsNotYetATenderer()
    {
        organisationClientMock
            .Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CO.CDP.Organisation.WebApiClient.Organisation(null!, null!, null!, null!, orgGuid, null!, "Org name", [PartyRole.Buyer], CDP.Organisation.WebApiClient.OrganisationType.Organisation));

        var result = await _model.OnGet(orgGuid);

        organisationClientMock.Verify(o => o.UpdateOrganisationAsync(
            orgGuid,
            It.Is<UpdatedOrganisation>(uo => uo.Organisation.Roles != null &&
                                              uo.Organisation.Roles.SequenceEqual(new[] { PartyRole.Tenderer }))),
            Times.Once);

        tempDataService.Verify(td => td.Put(FlashMessageTypes.Success, It.Is<FlashMessage>(fm => fm.Heading == "You have been registered as a supplier")));

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationOverview");
        redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public async Task ShouldNotUpdateOrganisation_WhenOrganisationIsAlreadyATenderer()
    {
        organisationClientMock
            .Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CO.CDP.Organisation.WebApiClient.Organisation(null!, null!, null!, null!, orgGuid, null!, "Org name", [PartyRole.Buyer, PartyRole.Tenderer], CDP.Organisation.WebApiClient.OrganisationType.Organisation));

        await _model.OnGet(orgGuid);

        organisationClientMock.Verify(o => o.UpdateOrganisationAsync(
            orgGuid,
            It.Is<UpdatedOrganisation>(uo => uo.Organisation.Roles != null &&
                                              uo.Organisation.Roles.SequenceEqual(new[] { PartyRole.Tenderer }))),
            Times.Never);
    }
}
