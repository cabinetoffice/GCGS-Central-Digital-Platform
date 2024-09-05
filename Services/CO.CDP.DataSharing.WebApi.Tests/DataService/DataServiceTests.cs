using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace DataSharing.Tests.DataService;

public class DataServiceTests
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private CO.CDP.DataSharing.WebApi.DataService.DataService DataService => new(_shareCodeRepository.Object);

    private SharedConsent CreateSharedConsent(
        string? shareCode = null
    )
    {
        return new SharedConsent
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
            ShareCode = shareCode ?? "valid-share-code",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    [Fact]
    public async Task GetSharedSupplierInformationAsync_ShouldReturnSharedSupplierInformation_WhenOrganisationExists()
    {
        var shareCode = "ABC-123";
        var sharedConsent = CreateSharedConsent(shareCode: shareCode);
        _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var result = await DataService.GetSharedSupplierInformationAsync(shareCode);

        result.Should().NotBeNull();
        result.BasicInformation.SupplierType.Should().Be(sharedConsent.Organisation.SupplierInfo?.SupplierType);
    }

    [Fact]
    public async Task
        GetSharedSupplierInformationAsync_ShouldThrowSupplierInformationNotFoundException_WhenSupplierInfoIsNull()
    {
        var shareCode = "ABC-123";
        var sharedConsent = CreateSharedConsent(shareCode: shareCode);
        sharedConsent.Organisation.SupplierInfo = null;

        _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(shareCode);

        await act.Should().ThrowAsync<SupplierInformationNotFoundException>()
            .WithMessage("Supplier information not found.");
    }

    [Fact]
    public async Task ShouldThrowShareCodeNotFoundException_WhenShareCodeDoesNotExist()
    {
        var shareCode = "invalid-sharecode";

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode))
            .ReturnsAsync((SharedConsent?)null);

        Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(shareCode);

        await act.Should().ThrowAsync<ShareCodeNotFoundException>();
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