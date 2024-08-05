using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FluentAssertions;
using Moq;

namespace DataSharing.Tests.UseCase;

public class DataSharingProfile : Profile
{
    public DataSharingProfile()
    {
        CreateMap<SharedConsent, ShareReceipt>()
           .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
           .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId))
           .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));
    }
}

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<DataSharingProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}

public class UpdateSharedConsentWithShareCodeUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private UpdateSharedConsentWithShareCodeUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoOrganisationIsFound()
    {
        var shareRequest = new ShareRequest
        {
            FormId = default,
            OrganisationId = default
        };

        var shareReceipt = await UseCase.Execute(shareRequest);

        shareReceipt.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheFoundOrganisation()
    {
        var formId = Guid.NewGuid();
        var formVersionId = string.Empty;

        var form = new Form
        {
            Guid = formId,
            Name = default,
            Version = default,
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
            Organisation = default,
            Form = form,
            AnswerSets = default,
            SubmissionState = default,
            SubmittedAt = default,
            FormVersionId = formVersionId,
            BookingReference = default
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