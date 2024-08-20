using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.DataSharing.WebApi.Extensions;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetShareCodesUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _formRepository = new();
    private GetShareCodesUseCase UseCase => new(_formRepository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsEmptyIfNoSharedCodeIsFound()
    {
        var found = await UseCase.Execute(It.IsAny<Guid>());

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task ItReturnsTheSharedCodeList()
    {
        var organisationId = 2;
        var organisationGuid = Guid.NewGuid();
        var formId = (Guid)default;

        var shareRequest = EntityFactory.GetShareRequest(organisationGuid: organisationGuid, formId: formId);
        var sharedConsent = EntityFactory.GetSharedConsent(organisationId: organisationId, organisationGuid: organisationGuid, formId: formId);

        var shareCode = ShareCodeExtensions.GenerateShareCode();
        sharedConsent.BookingReference = shareCode;
        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;

        _formRepository.Setup(r => r.GetShareCodesAsync(shareRequest.OrganisationId)).ReturnsAsync(new List<OrganisationInformation.Persistence.Forms.SharedConsent> { sharedConsent });

        var result = await UseCase.Execute(sharedConsent.Organisation.Guid);

        result?.Should().NotBeEmpty();
        result?.Should().HaveCount(1);

        result?.First().As<CO.CDP.DataSharing.WebApi.Model.SharedConsent>().SubmittedAt.Should().Be(sharedConsent.SubmittedAt);
        result?.First().As<CO.CDP.DataSharing.WebApi.Model.SharedConsent>().ShareCode.Should().Be(sharedConsent.BookingReference);
    }
}