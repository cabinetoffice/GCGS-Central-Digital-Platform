using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Xunit;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;
public class GetSharedDataUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private GetSharedDataUseCase UseCase => new(_shareCodeRepository.Object, mapperFixture.Mapper);


    [Fact]
    public async Task ItThrowsExceptionIfNoSharedConsentIsFound()
    {
        var response = async () => await UseCase.Execute(It.IsAny<String>());

        await response.Should().ThrowAsync<SharedConsentNotFoundException>();
    }

    [Fact]
    public async Task ItReturnsMappedSupplierInformationWhenSharedConsentIsFound()
    {
        string shareCode = "valid-sharecode";
        var organisationId = 1;
        var organisationGuid = Guid.NewGuid();
        var formId = Guid.NewGuid();

        var sharedConsent = EntityFactory.GetSharedConsent(organisationId, organisationGuid, formId);
        sharedConsent.ShareCode = shareCode;
        sharedConsent.Organisation.Name = "Test Organisation";

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var result = await UseCase.Execute(shareCode);

        result.Should().NotBeNull();
        result?.Name.Should().Be("Test Organisation");
        result?.Id.Should().Be(sharedConsent.Organisation.Guid);
        result?.AssociatedPersons.Should().HaveCount(1);
        result?.AssociatedPersons.First().Name.Should().Be("John Doe");
        result?.Identifier.Id.Should().Be("123456789");
        result?.Identifier.LegalName.Should().Be("Test Organisation Ltd.");
    }
}