using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FluentAssertions;
using Moq;
using CO.CDP.Authentication;

namespace DataSharing.Tests.UseCase;

public class GenerateShareCodeUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IClaimService> _claimService = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IFormRepository> _formRepository = new();
    private GenerateShareCodeUseCase UseCase => new(_claimService.Object, _organisationRepository.Object, _formRepository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ThrowsInvalidOrganisationRequestedException_WhenShareCodeRequestedForNonExistentOrganisation()
    {
        var organisationId = (int)default;
        var organisationGuid = (Guid)default;
        var formId = (Guid)default;

        var (shareRequest, sharedConsent) = EntityFactory.GenerateSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);


        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<InvalidOrganisationRequestedException>();
    }

    [Fact]
    public async Task ThrowsInvalidOrganisationRequestedException_WhenShareCodeRequestedForNotAuthorisedOrganisation()
    {
        var apiKeyOrganisationId = 1;
        var organisationId = 2;
        var organisationGuid = Guid.NewGuid();
        var formId = (Guid)default;

        var (shareRequest, sharedConsent) = EntityFactory.GenerateSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(sharedConsent.Organisation);

        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<InvalidOrganisationRequestedException>();
    }

    [Fact]
    public async Task ThrowsSharedConsentNotFoundException_WhenNoSharedConsentOrNoneInValidStateFound()
    {
        var apiKeyOrganisationId = 1;
        var organisationId = apiKeyOrganisationId;
        var organisationGuid = Guid.NewGuid();
        var formId = Guid.NewGuid();

        var (shareRequest, sharedConsent) = EntityFactory.GenerateSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(sharedConsent.Organisation);
        _formRepository.Setup(r => r.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId)).ReturnsAsync((SharedConsent?)null);

        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<SharedConsentNotFoundException>();
    }

    [Fact]
    public async Task ReturnsRelevantShareReceipt_WhenAuthorisedAndShareRequestForValidExistingShareConsentProvided()
    {
        var apiKeyOrganisationId = 1;
        var organisationId = apiKeyOrganisationId;
        var organisationGuid = Guid.NewGuid();
        var formId = Guid.NewGuid();

        var (shareRequest, sharedConsent) = EntityFactory.GenerateSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(sharedConsent.Organisation);
        _formRepository.Setup(r => r.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId)).ReturnsAsync(sharedConsent);

        var found = await UseCase.Execute(shareRequest);

        found.FormId.Should().Be(shareRequest.FormId);
        found.FormVersionId.Should().Be(sharedConsent.FormVersionId);
        found.ShareCode.Should().NotBeNullOrWhiteSpace();
    }
}