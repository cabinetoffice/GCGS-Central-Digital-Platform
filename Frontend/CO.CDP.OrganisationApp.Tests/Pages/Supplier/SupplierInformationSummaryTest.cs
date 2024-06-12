using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierInformationSummaryTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public SupplierInformationSummaryTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Theory]
    [InlineData(null, false, false, false, false, false, false, false, false, false, StepStatus.NotStarted)]
    [InlineData(SupplierType.Individual, true, false, false, false, false, false, false, false, false, StepStatus.InProcess)]
    [InlineData(SupplierType.Individual, true, true, true, true, true, true, true, false, false, StepStatus.Completed)]
    [InlineData(SupplierType.Organisation, true, true, true, true, true, true, true, true, true, StepStatus.Completed)]
    public async Task OnGet_BasicInformationStepStatus(
            SupplierType? supplierType,
            bool completedRegAddress,
            bool completedPostalAddress,
            bool completedVat,
            bool completedWebsiteAddress,
            bool completedEmailAddress,
            bool completedQualification,
            bool completedTradeAssurance,
            bool completedOperationType,
            bool completedLegalForm,
            StepStatus expectedStatus)
    {
        var model = GivenSupplierInformationSummaryModel();
        var supplierInformation = new SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: supplierType,
            operationTypes: null,
            completedRegAddress: completedRegAddress,
            completedPostalAddress: completedPostalAddress,
            completedVat: completedVat,
            completedWebsiteAddress: completedWebsiteAddress,
            completedEmailAddress: completedEmailAddress,
            completedQualification: completedQualification,
            completedTradeAssurance: completedTradeAssurance,
            completedOperationType: completedOperationType,
            completedLegalForm: completedLegalForm,
            vatNumber: null);

        organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(supplierInformation);

        await model.OnGet(Guid.NewGuid());

        model.Name.Should().Be("FakeOrg");
        model.BasicInformationStepStatus.Should().Be(expectedStatus);
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

    private SupplierInformationSummaryModel GivenSupplierInformationSummaryModel()
    {
        return new SupplierInformationSummaryModel(sessionMock.Object, organisationClientMock.Object);
    }
}
