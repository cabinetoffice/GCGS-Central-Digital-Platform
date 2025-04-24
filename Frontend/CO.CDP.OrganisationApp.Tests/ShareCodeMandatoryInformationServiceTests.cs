using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using FluentAssertions;
using FluentAssertions.Common;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;
public class ShareCodeMandatoryInformationServiceTests
{
    private readonly Mock<FormsClient> _formsClientMock;
    private readonly Mock<OrganisationClient> _organisationClientMock;
    private readonly ShareCodeMandatoryInformationService _service;
    public ShareCodeMandatoryInformationServiceTests()
    {
        _formsClientMock = new Mock<FormsClient>("https://whatever", new HttpClient());
        _organisationClientMock = new Mock<OrganisationClient>("https://whatever", new HttpClient());
        _service = new ShareCodeMandatoryInformationService(_organisationClientMock.Object, _formsClientMock.Object);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeTrue_WhenAllSectionsAreCompleted()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallOrMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(endDate: null, entityId: new Guid(), entityType: ConnectedEntityType.Organisation, name: "connected entity name", uri: new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _service.MandatorySectionsCompleted(organisationId)).Should().Be(true);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenBasicInfoIsIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, false, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallOrMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(endDate: null, entityId: new Guid(), entityType: ConnectedEntityType.Organisation, name: "connected entity name", uri: new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _service.MandatorySectionsCompleted(organisationId)).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenConnectedEntitiesIsIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(false, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallOrMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>());

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 3, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _service.MandatorySectionsCompleted(organisationId)).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeFalse_WhenFormSectionsAreIncomplete()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallOrMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(endDate: null, entityId: new Guid(), entityType: ConnectedEntityType.Organisation, name: "connected entity name", uri: new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 0, false, sectionId, "section name", FormSectionType.Standard) }));

        (await _service.MandatorySectionsCompleted(organisationId)).Should().Be(false);
    }

    [Fact]
    public async Task MandatorySectionsCompleted_ShouldBeTrue_WhenFormSectionsAreCompleteWithNoAnswers()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        _organisationClientMock
            .Setup(c => c.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(
                new SupplierInformation(true, true, true, true, true, true, true, true, new LegalForm("law", "form", true, new DateTimeOffset()), new List<OperationType>() { OperationType.SmallOrMediumSized }, "org name", SupplierType.Organisation));

        _organisationClientMock
            .Setup(c => c.GetConnectedEntitiesAsync(organisationId))
            .ReturnsAsync(new List<ConnectedEntityLookup>() { new ConnectedEntityLookup(endDate: null, entityId: new Guid(), entityType: ConnectedEntityType.Organisation, name: "connected entity name", uri: new Uri("http://whatever")) });

        _formsClientMock
            .Setup(f => f.GetFormSectionsAsync(It.IsAny<Guid>(), organisationId))
            .ReturnsAsync(new FormSectionResponse(new List<FormSectionSummary>() { new FormSectionSummary(true, 0, true, sectionId, "section name", FormSectionType.Standard) }));

        (await _service.MandatorySectionsCompleted(organisationId)).Should().Be(true);
    }
}