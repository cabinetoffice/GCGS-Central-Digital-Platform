using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierBasicInformationTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public SupplierBasicInformationTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public async Task OnGet_SetSupplierInformation()
    {
        var model = GivenSupplierInformationSummaryModel();
        var supplierInformation = new SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: false,
            completedWebsiteAddress: false,
            completedEmailAddress: false,
            completedQualification: false,
            completedTradeAssurance: false,
            completedOperationType: false,
            completedLegalForm: false,
            vatNumber: null);

        organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(supplierInformation);

        await model.OnGet(Guid.NewGuid());

        model.SupplierInformation.Should().Be(supplierInformation);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        var model = GivenSupplierInformationSummaryModel();

        organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private SupplierBasicInformationModel GivenSupplierInformationSummaryModel()
    {
        return new SupplierBasicInformationModel(sessionMock.Object, organisationClientMock.Object);
    }
}