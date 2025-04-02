using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class AddOrganisationPartyUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _orgRepoMock = new();
    private readonly Mock<IShareCodeRepository> _shareCodeRepoMock = new();
    private readonly Mock<IOrganisationPartiesRepository> _orgPartiesRepoMock = new();
    private AddOrganisationPartyUseCase UseCase => new(_orgRepoMock.Object, _shareCodeRepoMock.Object, _orgPartiesRepoMock.Object);

    [Fact]
    public async Task Execute_ShouldThrowException_WhenParentOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid(),
            ShareCode = null,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenChildOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = null,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {childOrganisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenShareCodeIsInvalid()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId);
        var shareCode = "InvalidCode";
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = shareCode,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync(childOrg);
        _shareCodeRepoMock.Setup(repo => repo.GetShareCodesAsync(childOrganisationId)).ReturnsAsync([]);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<OrganisationShareCodeInvalid>()
            .WithMessage($"Invalid organisation share code: {shareCode}");
    }

    [Fact]
    public async Task Execute_ShouldSaveOrganisationParty_WhenValidInputProvided()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId);
        var shareCode = "ValidCode";
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
            ShareCode = shareCode ?? "valid-sharecode",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync(childOrg);
        _shareCodeRepoMock.Setup(repo => repo.GetShareCodesAsync(childOrganisationId)).ReturnsAsync([sharedConsent]);

        var result = await UseCase.Execute((organisationId, new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = shareCode,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        }));

        result.Should().BeTrue();

        _orgPartiesRepoMock.Verify(repo => repo.Save(It.Is<Persistence.OrganisationParty>(party =>
            party.ParentOrganisationId == parentOrg.Id &&
            party.ChildOrganisationId == childOrg.Id &&
            party.SharedConsentId == sharedConsent.Id &&
            party.OrganisationRelationship == Persistence.OrganisationRelationship.Consortium)), Times.Once);
    }

    private static Persistence.Organisation GivenOrganisation(Guid guid) =>
        new()
        {
            Guid = guid,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };
}