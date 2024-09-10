using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using static DataSharing.Tests.DataSharingFactory;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace DataSharing.Tests.DataService;

public class DataServiceTests
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IConnectedEntityRepository> _connectedEntityRepository = new();
    private CO.CDP.DataSharing.WebApi.DataService.DataService DataService => new(_shareCodeRepository.Object,
        _connectedEntityRepository.Object);

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
    public async Task ShouldMapOrganisationToBasicInformation()
    {
        var sharedConsent = CreateSharedConsent();
        var organisation = sharedConsent.Organisation;

        _shareCodeRepository.Setup(r => r.GetByShareCode("ABC-123")).ReturnsAsync(sharedConsent);

        var result = await DataService.GetSharedSupplierInformationAsync("ABC-123");

        result.BasicInformation.SupplierType.Should().Be(organisation.SupplierInfo?.SupplierType);
        result.BasicInformation.RegisteredAddress.Should().NotBeNull();
        result.BasicInformation.PostalAddress.Should().NotBeNull();
        result.BasicInformation.VatNumber.Should()
            .Be(organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId);
        result.BasicInformation.WebsiteAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Url);
        result.BasicInformation.EmailAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Email);
        result.BasicInformation.Qualifications.Should().HaveCount(1);
        result.BasicInformation.TradeAssurances.Should().HaveCount(1);
        result.BasicInformation.LegalForm.Should().NotBeNull();
    }
}