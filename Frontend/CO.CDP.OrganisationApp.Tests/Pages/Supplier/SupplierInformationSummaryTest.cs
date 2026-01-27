using CO.CDP.DataSharing.WebApiClient;
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
    private readonly Mock<IDataSharingClient> _dataSharingClient;
    private readonly SupplierInformationSummaryModel _model;

    public SupplierInformationSummaryTest()
    {
        _organisationClientMock = new();
        _formClient = new();
        _formClient.Setup(o => o.GetFormSectionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new FormSectionResponse([]));
        _dataSharingClient = new();
        _dataSharingClient.Setup(DataSharingClient => DataSharingClient.GetShareCodeListAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<SharedConsent>());
        _model = new SupplierInformationSummaryModel(_organisationClientMock.Object, _formClient.Object, _dataSharingClient.Object);
    }

    [Theory]
    [InlineData(null, false, false, false, false, false, false, false, false, SupplierInformationStatus.StepStatus.NotStarted, SupplierInformationStatus.StepStatus.NotStarted)]
    [InlineData(SupplierType.Individual, true, false, false, false, false, false, false, false, SupplierInformationStatus.StepStatus.InProcess, SupplierInformationStatus.StepStatus.NotStarted)]
    [InlineData(SupplierType.Individual, true, true, true, true, true, false, false, false, SupplierInformationStatus.StepStatus.Completed, SupplierInformationStatus.StepStatus.NotStarted)]
    [InlineData(SupplierType.Organisation, true, true, true, true, true, true, true, true, SupplierInformationStatus.StepStatus.Completed, SupplierInformationStatus.StepStatus.Completed)]
    public async Task OnGet_BasicInformationStepStatus(
            SupplierType? supplierType,
            bool completedRegAddress,
            bool completedPostalAddress,
            bool completedVat,
            bool completedWebsiteAddress,
            bool completedEmailAddress,
            bool completedOperationType,
            bool completedLegalForm,
            bool completedConnectedPerson,
            SupplierInformationStatus.StepStatus expectedStatusSupplier,
            SupplierInformationStatus.StepStatus expectedStatusConnectedPerson)
    {
        var supplierInformation = new CDP.Organisation.WebApiClient.SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: supplierType,
            operationTypes: null,
            completedRegAddress: completedRegAddress,
            completedPostalAddress: completedPostalAddress,
            completedVat: completedVat,
            completedWebsiteAddress: completedWebsiteAddress,
            completedEmailAddress: completedEmailAddress,
            completedOperationType: completedOperationType,
            completedLegalForm: completedLegalForm,
            completedConnectedPerson: completedConnectedPerson,
            legalForm: null);

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
    public async Task OnGet_WhenNoOrganisationFound_ShouldRedirectsToPageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_WhenNoFormFound_ShouldRedirectsToPageNotFound()
    {
        _formClient.Setup(o => o.GetFormSectionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new CDP.Forms.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_PageNotFoundOnConnectedEntities()
    {
        _organisationClientMock.Setup(o => o.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Theory]
    [InlineData("organisation-home", "/organisation/{0}/home")]
    [InlineData(null, "/organisation/{0}")]
    [InlineData("", "/organisation/{0}")]
    [InlineData("some-other-value", "/organisation/{0}")]
    public async Task OnGet_ShouldSetBackLinkUrlBasedOnOrigin(string? origin, string expectedUrlPattern)
    {
        var organisationId = Guid.NewGuid();
        var supplierInformation = new CDP.Organisation.WebApiClient.SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: true,
            completedVat: true,
            completedWebsiteAddress: true,
            completedEmailAddress: true,
            completedOperationType: true,
            completedLegalForm: true,
            completedConnectedPerson: true,
            legalForm: null);

        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(supplierInformation);
        _organisationClientMock.Setup(o => o.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(ConnectedEntities);

        _model.Origin = origin;
        _model.Id = organisationId;

        await _model.OnGet(organisationId);

        var expectedUrl = string.Format(expectedUrlPattern, organisationId);
        _model.BackLinkUrl.Should().Be(expectedUrl);
    }

    private static List<ConnectedEntityLookup> ConnectedEntities =>
    [
        new(endDate: It.IsAny<DateTimeOffset>(), entityId: Guid.NewGuid(), entityType: ConnectedEntityType.Organisation, name: "e1", uri: It.IsAny<Uri>(), deleted: false, isInUse: false, formGuid: null, sectionGuid: null),
        new(endDate: It.IsAny<DateTimeOffset>(), entityId: Guid.NewGuid(), entityType: ConnectedEntityType.Organisation, name: "e2", uri: It.IsAny<Uri>(), deleted: false, isInUse: false, formGuid: null, sectionGuid: null),
    ];
}