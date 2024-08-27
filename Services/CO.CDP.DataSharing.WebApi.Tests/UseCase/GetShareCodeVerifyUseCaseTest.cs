using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.Extensions;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetShareCodeVerifyUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _formRepository = new();
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private GetShareCodeVerifyUseCase UseCase => new(_shareCodeRepository.Object);

    [Fact]
    public async Task ThrowsSharedConsentNotFoundException_WhenShareCodeRequestedNotFound()
    {
        var formVersionId = "default";
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);

        var response = async () => await UseCase.Execute(shareVerificationRequest);

        await response.Should().ThrowAsync<SharedConsentNotFoundException>();
    }

    [Fact]
    public async Task ReturnsTrue_WhenShareVerificationRequestForValidExistingShareCodeAndFormVersion()
    {
        var formVersionId = "V1.0";
        var shareCode = ShareCodeExtensions.GenerateShareCode();
        
        var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);      

        _shareCodeRepository.Setup(r => r.GetShareCodeVerifyAsync(shareVerificationRequest.FormVersionId, shareVerificationRequest.ShareCode)).ReturnsAsync(true);

        var response = await UseCase.Execute(shareVerificationRequest);

        response.Should().NotBeNull();

        response.As<CO.CDP.DataSharing.WebApi.Model.ShareVerificationReceipt>().ShareCode.Should().Be(shareVerificationRequest.ShareCode);
        response.As<CO.CDP.DataSharing.WebApi.Model.ShareVerificationReceipt>().FormVersionId.Should().Be(shareVerificationRequest.FormVersionId);
        response.As<CO.CDP.DataSharing.WebApi.Model.ShareVerificationReceipt>().IsLatest.Should().Be(true);        
    }
}