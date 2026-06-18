using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using FluentAssertions;
using Moq;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class BulkShareCodeLookupUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private BulkShareCodeLookupUseCase UseCase => new(_shareCodeRepository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsAnInvalidResultForEveryShareCodeWhenNoneAreFound()
    {
        _shareCodeRepository
            .Setup(r => r.GetByShareCodes(It.IsAny<ICollection<string>>()))
            .ReturnsAsync([]);

        var request = new BulkShareCodeLookupRequest { ShareCodes = ["MISSING-1", "MISSING-2"] };

        var results = (await UseCase.Execute(request)).ToList();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.IsValid == false);
        results.Select(r => r.ShareCode).Should().ContainInOrder("MISSING-1", "MISSING-2");
        results.Should().OnlyContain(r =>
            r.OrganisationId == null && r.OrganisationName == null && r.SubmittedAt == null);
    }

    [Fact]
    public async Task ItMapsTheSupplierInformationForFoundShareCodes()
    {
        var organisationGuid = Guid.NewGuid();
        var submittedAt = DateTimeOffset.UtcNow;

        var sharedConsent = NonEfEntityFactory.GetSharedConsent(organisationGuid, Guid.NewGuid());
        sharedConsent.ShareCode = "VALID-CODE";
        sharedConsent.SubmittedAt = submittedAt;

        _shareCodeRepository
            .Setup(r => r.GetByShareCodes(It.IsAny<ICollection<string>>()))
            .ReturnsAsync([sharedConsent]);

        var request = new BulkShareCodeLookupRequest { ShareCodes = ["VALID-CODE"] };

        var results = (await UseCase.Execute(request)).ToList();

        results.Should().ContainSingle();
        var result = results.Single();

        result.ShareCode.Should().Be("VALID-CODE");
        result.IsValid.Should().BeTrue();
        result.OrganisationId.Should().Be(organisationGuid);
        result.OrganisationName.Should().Be("Test Organisation");
        result.SubmittedAt.Should().Be(submittedAt);
        result.Identifier.LegalName.Should().Be("DefaultLegalName");
        result.AdditionalIdentifiers.Should().ContainSingle(i => i.Scheme == "GB-COH");
        result.Address.Should().NotBeNull();
        result.Address.StreetAddress.Should().Be("1234 Default St");
        result.AdditionalAddresses.Should().ContainSingle(a => a.Type == AddressType.Postal);
        result.ContactPoint.Name.Should().Be("Default Contact");
    }

    [Fact]
    public async Task ItPreservesRequestOrderAndMarksMissingShareCodesAsInvalid()
    {
        var foundConsent = NonEfEntityFactory.GetSharedConsent(Guid.NewGuid(), Guid.NewGuid());
        foundConsent.ShareCode = "FOUND";

        _shareCodeRepository
            .Setup(r => r.GetByShareCodes(It.IsAny<ICollection<string>>()))
            .ReturnsAsync([foundConsent]);

        var request = new BulkShareCodeLookupRequest { ShareCodes = ["MISSING-BEFORE", "FOUND", "MISSING-AFTER"] };

        var results = (await UseCase.Execute(request)).ToList();

        results.Select(r => r.ShareCode).Should().ContainInOrder("MISSING-BEFORE", "FOUND", "MISSING-AFTER");
        results.Select(r => r.IsValid).Should().ContainInOrder(false, true, false);
    }

    [Fact]
    public async Task ItLooksUpAllShareCodesInASingleRepositoryCall()
    {
        _shareCodeRepository
            .Setup(r => r.GetByShareCodes(It.IsAny<ICollection<string>>()))
            .ReturnsAsync([]);

        var request = new BulkShareCodeLookupRequest { ShareCodes = ["A", "B", "C"] };

        await UseCase.Execute(request);

        _shareCodeRepository.Verify(
            r => r.GetByShareCodes(It.Is<ICollection<string>>(c => c.Count == 3)), Times.Once);
        _shareCodeRepository.Verify(r => r.GetByShareCode(It.IsAny<string>()), Times.Never);
    }
}
