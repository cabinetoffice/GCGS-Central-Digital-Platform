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

        var form = new Form
        {
            Guid = formId,
            Name = string.Empty,
            Version = string.Empty,
            IsRequired = default,
            Type = default,
            Scope = default,
            Sections = new List<FormSection> { }
        };

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
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = default,
            SubmittedAt = default,
            FormVersionId = formVersionId,
            BookingReference = string.Empty
        };

        _repository.Setup(r => r.GetSharedConsentAsync(shareRequest.FormId, shareRequest.OrganisationId)).ReturnsAsync(sharedConsent);

        var found = await UseCase.Execute(shareRequest);
        var expectedShareReceipt = new ShareReceipt()
        {
            FormId = formId,
            FormVersionId = formVersionId,
            ShareCode = default
        };

        found.FormId.Should().Be(formId);
        found.FormVersionId.Should().Be(formVersionId);
        found.ShareCode.Should().NotBeNullOrWhiteSpace();
    }
}