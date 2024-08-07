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
        var shareRequest = new ShareRequest
        {
            FormId = default,
            OrganisationId = default
        };
        
        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<InvalidOrganisationRequestedException>();
    }

    [Fact]
    public async Task ThrowsInvalidOrganisationRequestedException_WhenShareCodeRequestedForNotAuthorisedOrganisation()
    {
        var apiKeyOrganisationId = 1;
        var organisationId = 2;
        var organisationGuid = Guid.NewGuid();
        var foundOrganisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            }
        };

        var shareRequest = new ShareRequest
        {
            FormId = default,
            OrganisationId = organisationGuid
        };

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(foundOrganisation);

        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<InvalidOrganisationRequestedException>();
    }

    [Fact]
    public async Task ThrowsSharedConsentNotFoundException_WhenNoSharedConsentOrNoneInValidStateFound()
    {
        var apiKeyOrganisationId = 1;
        var organisationId = apiKeyOrganisationId;
        var organisationGuid = Guid.NewGuid();
        var foundOrganisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            }
        };

        var formId = Guid.NewGuid();
        var formVersionId = string.Empty;

        var shareRequest = new ShareRequest
        {
            FormId = formId,
            OrganisationId = organisationGuid
        };

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(foundOrganisation);
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
        var foundOrganisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            }
        };

        var formId = Guid.NewGuid();
        var formVersionId = string.Empty;

        var shareRequest = new ShareRequest
        {
            FormId = formId,
            OrganisationId = organisationGuid
        };

        var sharedConsent = new SharedConsent()
        {
            Guid = formId,
            Organisation = foundOrganisation,
            Form = new Form
            {
                Guid = formId,
                Name = string.Empty,
                Version = string.Empty,
                IsRequired = default,
                Type = default,
                Scope = default,
                Sections = new List<FormSection> { }
            },
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = formVersionId,
            BookingReference = string.Empty
        };

        _claimService.Setup(x => x.GetOrganisationId()).Returns(apiKeyOrganisationId);
        _organisationRepository.Setup(x => x.Find(organisationGuid)).ReturnsAsync(foundOrganisation);
        _formRepository.Setup(r => r.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId)).ReturnsAsync(sharedConsent);

        var found = await UseCase.Execute(shareRequest);

        found.FormId.Should().Be(formId);
        found.FormVersionId.Should().Be(formVersionId);
        found.ShareCode.Should().NotBeNullOrWhiteSpace();
    }
}