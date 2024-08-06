using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FluentAssertions;
using Moq;

namespace DataSharing.Tests.UseCase;

public class GenerateShareCodeUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private GenerateShareCodeUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ReturnsNullWhenNoRelevantSharedConsentFound()
    {
        var shareRequest = new ShareRequest
        {
            FormId = default,
            OrganisationId = default
        };

        var shareReceipt = async () => await UseCase.Execute(shareRequest);

        await shareReceipt.Should().ThrowAsync<SharedConsentNotFoundException>();
    }

    [Fact]
    public async Task ReturnsRelevantShareReceipt()
    {
        var formId = Guid.NewGuid();
        var formVersionId = string.Empty;

        var shareRequest = new ShareRequest
        {
            FormId = formId,
            OrganisationId = Guid.NewGuid()
        };

        var sharedConsent = new SharedConsent()
        {
            Guid = formId,
            Organisation = new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty,
                Tenant = new Tenant
                {
                    Guid = Guid.NewGuid(),
                    Name = string.Empty
                }
            },
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
            SubmissionState = SubmissionState.Submitted,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = formVersionId,
            BookingReference = string.Empty
        };

        _repository.Setup(r => r.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId)).ReturnsAsync(sharedConsent);

        var found = await UseCase.Execute(shareRequest);

        found.FormId.Should().Be(formId);
        found.FormVersionId.Should().Be(formVersionId);
        found.ShareCode.Should().NotBeNullOrWhiteSpace();
    }
}