using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using CO.CDP.DataSharing.WebApi.Extensions;


namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetShareCodeDetailsUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private GetShareCodeDetailsUseCase UseCase => new(_shareCodeRepository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoSharedCodeIsFound()
    {
        var found = await UseCase.Execute((It.IsAny<Guid>(), It.IsAny<string>()));

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheSharedCodeList()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        var sharedConsentDetails = new CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsentDetails
        {
            ShareCode = shareCode,
            SubmittedAt=DateTime.UtcNow,
            QuestionAnswers=new List<CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsentQuestionAnswer>()
        };

        _shareCodeRepository.Setup(r => r.GetShareCodeDetailsAsync(organisationId, shareCode)).ReturnsAsync(sharedConsentDetails);

        var result = await UseCase.Execute((organisationId, shareCode));

        result.Should().NotBeNull();

        result.As<CO.CDP.DataSharing.WebApi.Model.SharedConsentDetails>().SubmittedAt.Should().Be(sharedConsentDetails.SubmittedAt);
        result.As<CO.CDP.DataSharing.WebApi.Model.SharedConsentDetails>().ShareCode.Should().Be(sharedConsentDetails.ShareCode);
    }
}