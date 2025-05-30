using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using OrganisationParty = CO.CDP.OrganisationInformation.Persistence.OrganisationParty;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateOrganisationPartyUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _mockOrgRepo;
    private readonly Mock<IShareCodeRepository> _mockShareCodeRepo;
    private readonly Mock<IOrganisationPartiesRepository> _mockOrgPartiesRepo;
    private readonly UpdateOrganisationPartyUseCase _useCase;

    public UpdateOrganisationPartyUseCaseTests()
    {
        _mockOrgRepo = new Mock<IOrganisationRepository>();
        _mockShareCodeRepo = new Mock<IShareCodeRepository>();
        _mockOrgPartiesRepo = new Mock<IOrganisationPartiesRepository>();

        _useCase = new UpdateOrganisationPartyUseCase(
            _mockOrgRepo.Object,
            _mockShareCodeRepo.Object,
            _mockOrgPartiesRepo.Object
        );
    }

    [Fact]
    public async Task Execute_ShouldUpdateSharedConsent_WhenValidShareCodeIsProvided()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId, id: 1);
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId, id: 2);
        var sharedConsent = new Persistence.Forms.SharedConsent
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            OrganisationId = childOrg.Id,
            Organisation = childOrg,
            FormId = 1,
            Form = null!,
            AnswerSets = [],
            SubmissionState = Persistence.Forms.SubmissionState.Submitted,
            SubmittedAt = null,
            FormVersionId = string.Empty,
            ShareCode = "valid-share-code",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
        };

        var consortium = new List<OrganisationParty>
        {
            new OrganisationParty { ParentOrganisationId = 1, ChildOrganisationId = 2, OrganisationRelationship = Persistence.OrganisationRelationship.Consortium }
        };

        _mockOrgRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(parentOrg);

        _mockOrgRepo.Setup(repo => repo.Find(childOrganisationId))
            .ReturnsAsync(childOrg);

        _mockShareCodeRepo.Setup(repo => repo.GetShareCodesAsync(childOrganisationId))
            .ReturnsAsync(new List<Persistence.Forms.SharedConsent> { sharedConsent });

        _mockOrgPartiesRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(consortium);

        var updateParty = new UpdateOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = sharedConsent.ShareCode
        };

        var result = await _useCase.Execute((organisationId, updateParty));

        result.Should().BeTrue();

        _mockOrgPartiesRepo.Verify(repo => repo.Save(It.Is<OrganisationParty>(
            p => p.SharedConsentId == sharedConsent.Id
        )), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenParentOrganisationDoesNotExist()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId, id: 1);
        var updateParty = new UpdateOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid(),
            ShareCode = "VALIDCODE"
        };

        _mockOrgRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(parentOrg);

        var unknownOrgId = Guid.NewGuid();
        Func<Task> action = async () => await _useCase.Execute((unknownOrgId, updateParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown parent organisation {unknownOrgId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenChildOrganisationDoesNotExist()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId, id: 1);
        var updateParty = new UpdateOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid(),
            ShareCode = "VALIDCODE"
        };
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId, id: 2);

        _mockOrgRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(parentOrg);

        _mockOrgRepo.Setup(repo => repo.Find(Guid.NewGuid()))
            .ReturnsAsync(childOrg);

        var unknownOrgId = Guid.NewGuid();
        Func<Task> action = async () => await _useCase.Execute((organisationId, updateParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown child organisation {updateParty.OrganisationPartyId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenShareCodeIsInvalid()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId);
        var shareCode = "InvalidCode";
        var addParty = new UpdateOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = shareCode
        };

        _mockOrgRepo.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _mockOrgRepo.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync(childOrg);
        _mockShareCodeRepo.Setup(repo => repo.GetShareCodesAsync(childOrganisationId)).ReturnsAsync([]);

        Func<Task> action = async () => await _useCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<OrganisationShareCodeInvalid>()
            .WithMessage($"Invalid organisation share code: {shareCode}");
    }

    private static Persistence.Organisation GivenOrganisation(Guid guid, int id = 0) =>
        new()
        {
            Id = id,
            Guid = guid,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };
}