using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierInformationSummaryTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierInformationSummaryModel _model;

    public SupplierInformationSummaryTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierInformationSummaryModel(_sessionMock.Object, _organisationClientMock.Object);
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
            tradeAssurances: null,
            legalForm: null,
            qualifications: null);

        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(supplierInformation);

        await _model.OnGet(Guid.NewGuid());

        _model.Name.Should().Be("FakeOrg");
        _model.BasicInformationStepStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }
}
