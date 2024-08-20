using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierInformationSummaryTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IFormsClient> _formClient;
    private readonly SupplierInformationSummaryModel _model;

    public SupplierInformationSummaryTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _formClient = new();
        _model = new SupplierInformationSummaryModel(_organisationClientMock.Object, _formClient.Object);
    }

    [Theory]
    [InlineData(null, false, false, false, false, false, false, false, false, false, false, StepStatus.NotStarted, StepStatus.NotStarted)]
    [InlineData(SupplierType.Individual, true, false, false, false, false, false, false, false, false, false, StepStatus.InProcess, StepStatus.NotStarted)]
    [InlineData(SupplierType.Individual, true, true, true, true, true, true, true, false, false, false, StepStatus.Completed, StepStatus.NotStarted)]
    [InlineData(SupplierType.Organisation, true, true, true, true, true, true, true, true, true, true, StepStatus.Completed, StepStatus.Completed)]
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
            bool completedConnectedPerson,
            StepStatus expectedStatusSupplier,
            StepStatus expectedStatusConnectedPerson)
    {
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
            completedConnectedPerson: completedConnectedPerson,
            tradeAssurances: null,
            legalForm: null,
            qualifications: null);

        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(supplierInformation);
        _organisationClientMock.Setup(o => o.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ConnectedEntities);

        await _model.OnGet(Guid.NewGuid());

        _model.Name.Should().Be("FakeOrg");
        _model.BasicInformationStepStatus.Should().Be(expectedStatusSupplier);
        _model.ConnectedPersonStepStatus.Should().Be(expectedStatusConnectedPerson);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_PageNotFoundOnConnectedEntities()
    {
        _organisationClientMock.Setup(o => o.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static List<ConnectedEntityLookup> ConnectedEntities =>
    [
         new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e1", It.IsAny<Uri>()),
        new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e2", It.IsAny<Uri>()),
    ];
}