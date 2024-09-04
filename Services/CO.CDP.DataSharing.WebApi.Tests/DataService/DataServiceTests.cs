using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace DataSharing.Tests.DataService;
public class DataServiceTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private CO.CDP.DataSharing.WebApi.DataService.DataService DataService => new(_organisationRepository.Object);

    private CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent CreateMockSharedConsent()
    {
        return new CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            OrganisationId = 1,
            Organisation = DataSharingFactory.CreateMockOrganisation(),
            FormId = 1,
            Form = null!,
            AnswerSets = null!,
            SubmissionState = default,
            SubmittedAt = null,
            FormVersionId = string.Empty,
            ShareCode = "valid-sharecode",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    [Fact]
    public async Task GetSharedSupplierInformationAsync_ShouldReturnSharedSupplierInformation_WhenOrganisationExists()
    {
        var sharedConsent = CreateMockSharedConsent();
        _organisationRepository.Setup(repo => repo.Find(sharedConsent.OrganisationId))
            .ReturnsAsync(sharedConsent.Organisation);

        var result = await DataService.GetSharedSupplierInformationAsync(sharedConsent);

        result.Should().NotBeNull();
        result.BasicInformation.SupplierType.Should().Be(sharedConsent.Organisation.SupplierInfo?.SupplierType);
    }

    [Fact]
    public async Task GetSharedSupplierInformationAsync_ShouldThrowSupplierInformationNotFoundException_WhenSupplierInfoIsNull()
    {
        var sharedConsent = CreateMockSharedConsent();
        sharedConsent.Organisation.SupplierInfo = null;

        _organisationRepository.Setup(repo => repo.Find(sharedConsent.OrganisationId))
            .ReturnsAsync(sharedConsent.Organisation);

        Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(sharedConsent);

        await act.Should().ThrowAsync<SupplierInformationNotFoundException>()
            .WithMessage("Supplier information not found.");
    }

    [Fact]
    public void MapToBasicInformation_ShouldMapOrganisationToBasicInformationCorrectly()
    {
        var organisation = DataSharingFactory.CreateMockOrganisation();

        var result = DataService.MapToBasicInformation(organisation);

        result.Should().NotBeNull();
        result.SupplierType.Should().Be(organisation.SupplierInfo?.SupplierType);
        result.RegisteredAddress.Should().NotBeNull();
        result.PostalAddress.Should().NotBeNull();
        result.VatNumber.Should().Be(organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId);
        result.WebsiteAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Url);
        result.EmailAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Email);
        result.Qualifications.Should().HaveCount(1);
        result.TradeAssurances.Should().HaveCount(1);
        result.LegalForm.Should().NotBeNull();
    }
}
