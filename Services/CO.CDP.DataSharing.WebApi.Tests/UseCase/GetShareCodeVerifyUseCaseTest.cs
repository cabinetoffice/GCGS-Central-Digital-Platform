using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.Authentication;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetShareCodeVerifyUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IClaimService> _claimService = new();
    private GetShareCodeVerifyUseCase UseCase => new(_shareCodeRepository.Object, _claimService.Object);

    [Fact]
    public async Task ThrowsUserUnauthorizedException_WhenRequestedUserOrganisationAndShareCodeRequestedNotFound()
    {
        var shareCode = "dummy_code";

        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, shareCode)).ReturnsAsync(false);

        var response = async () => await UseCase.Execute(
            new ShareVerificationRequest { FormVersionId = "1.0", ShareCode = shareCode });

        await response.Should().ThrowAsync<UserUnauthorizedException>();
    }

    [Fact]
    public async Task ThrowsUserUnauthorizedException_WhenRequestedUserOrganisationInNotInTheClaim()
    {
        _claimService.Setup(c => c.GetOrganisationId()).Returns((Guid?)null);

        var response = async () => await UseCase.Execute(
            new ShareVerificationRequest { FormVersionId = "1.0", ShareCode = "dummy_code" });

        await response.Should().ThrowAsync<UserUnauthorizedException>();
    }

    [Fact]
    public async Task ThrowsSharedConsentNotFoundException_WhenShareCodeRequestedNotFound()
    {
        var formVersionId = "default";
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, shareCode)).ReturnsAsync(true);

        var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);

        var response = async () => await UseCase.Execute(shareVerificationRequest);

        await response.Should().ThrowAsync<ShareCodeNotFoundException>();
    }

    [Fact]
    public async Task ReturnsTrue_WhenShareVerificationRequestForValidExistingShareCodeAndFormVersion()
    {
        var formVersionId = "V1.0";
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, shareCode)).ReturnsAsync(true);

        var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);

        _shareCodeRepository.Setup(r => r.GetShareCodeVerifyAsync(shareVerificationRequest.FormVersionId, shareVerificationRequest.ShareCode)).ReturnsAsync(true);

        var response = await UseCase.Execute(shareVerificationRequest);

        response.Should().NotBeNull();

        response.As<ShareVerificationReceipt>().ShareCode.Should().Be(shareVerificationRequest.ShareCode);
        response.As<ShareVerificationReceipt>().FormVersionId.Should().Be(shareVerificationRequest.FormVersionId);
        response.As<ShareVerificationReceipt>().IsLatest.Should().Be(true);
    }
}