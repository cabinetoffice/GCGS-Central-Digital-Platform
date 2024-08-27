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
    private GetShareCodeVerifyUseCase UseCase => new(_formRepository.Object);

    [Fact]
    public async Task ThrowsSharedConsentNotFoundException_WhenShareCodeRequestedNotFound()
    {
        var formVersionId = "default";
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);

        var response = async () => await UseCase.Execute(shareVerificationRequest);

        await response.Should().ThrowAsync<SharedConsentNotFoundException>();
    }

    //[Fact]
    //public async Task ReturnsTrue_WhenShareVerificationRequestForValidExistingShareCodeAndFormVersion()
    //{
    //    var formVersionId = "default";
    //    var shareCode = ShareCodeExtensions.GenerateShareCode();
    //    var organisationId = 1;
    //    var organisationGuid = Guid.NewGuid();
    //    var formId = Guid.NewGuid();

    //    var shareVerificationRequest = EntityFactory.GetShareVerificationRequest(formVersionId: formVersionId, shareCode: shareCode);
    //    var sharedConsent = EntityFactory.GetSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);
    //}
}