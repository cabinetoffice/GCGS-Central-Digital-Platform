using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using OrganisationParty = CO.CDP.OrganisationInformation.Persistence.OrganisationParty;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RemoveOrganisationPartyUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _orgRepoMock = new();
    private readonly Mock<IOrganisationPartiesRepository> _orgPartiesRepoMock = new();
    private readonly RemoveOrganisationPartyUseCase _useCase;

    public RemoveOrganisationPartyUseCaseTests()
    {
        _useCase = new RemoveOrganisationPartyUseCase(_orgRepoMock.Object, _orgPartiesRepoMock.Object);
    }

    private static Persistence.Organisation GivenOrganisation(Guid guid, int id) =>
        new()
        {
            Id = id,
            Guid = guid,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };

    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenRemovalSucceeds()
    {
        var orgId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var childParty = new OrganisationParty
        {
            ParentOrganisationId = 1,
            ChildOrganisationId = 2,
            OrganisationRelationship = Persistence.OrganisationRelationship.Consortium
        };
        var partyList = new List<OrganisationParty> { childParty };

        _orgRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync(GivenOrganisation(orgId, 1));
        _orgRepoMock.Setup(r => r.Find(childId)).ReturnsAsync(GivenOrganisation(childId, 2));
        _orgPartiesRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync(partyList);

        _orgPartiesRepoMock.Setup(r => r.Remove(childParty)).Returns(Task.CompletedTask);

        var input = (orgId, new RemoveOrganisationParty { OrganisationPartyId = childId });

        var result = await _useCase.Execute(input);

        result.Should().BeTrue();
        _orgPartiesRepoMock.Verify(r => r.Remove(childParty), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrow_WhenParentOrganisationNotFound()
    {
        var orgId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _orgRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync((Persistence.Organisation?)null);

        var input = (orgId, new RemoveOrganisationParty { OrganisationPartyId = childId });

        var act = async () => await _useCase.Execute(input);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown parent organisation {orgId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrow_WhenChildOrganisationNotFound()
    {
        var orgId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _orgRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync(GivenOrganisation(orgId, 1));
        _orgRepoMock.Setup(r => r.Find(childId)).ReturnsAsync((Persistence.Organisation?)null);

        var input = (orgId, new RemoveOrganisationParty { OrganisationPartyId = childId });

        var act = async () => await _useCase.Execute(input);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown child organisation {childId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrow_WhenConsortiumNotFound()
    {
        var orgId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _orgRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync(GivenOrganisation(orgId, 1));
        _orgRepoMock.Setup(r => r.Find(childId)).ReturnsAsync(GivenOrganisation(childId, 2));

        _orgPartiesRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync([]);

        var input = (orgId, new RemoveOrganisationParty { OrganisationPartyId = childId });

        var act = async () => await _useCase.Execute(input);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation child party {orgId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrow_WhenChildPartyNotFoundInList()
    {
        var orgId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _orgRepoMock.Setup(r => r.Find(orgId)).ReturnsAsync(GivenOrganisation(orgId, 1));
        _orgRepoMock.Setup(r => r.Find(childId)).ReturnsAsync(GivenOrganisation(childId, 2));

        var childParty = new OrganisationParty
        {
            ParentOrganisationId = 1,
            ChildOrganisationId = 3,
            OrganisationRelationship = Persistence.OrganisationRelationship.Consortium
        };
        var partyList = new List<OrganisationParty> { childParty };

        var input = (orgId, new RemoveOrganisationParty { OrganisationPartyId = childId });

        var act = async () => await _useCase.Execute(input);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation child party {orgId}.");
    }
}