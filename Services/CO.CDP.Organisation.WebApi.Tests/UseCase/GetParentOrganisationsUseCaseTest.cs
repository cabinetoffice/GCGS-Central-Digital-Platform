using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetParentOrganisationsUseCaseTest
{
    private readonly Mock<IOrganisationHierarchyRepository> _mockHierarchyRepository;
    private readonly GetParentOrganisationsUseCase _useCase;

    public GetParentOrganisationsUseCaseTest()
    {
        _mockHierarchyRepository = new Mock<IOrganisationHierarchyRepository>();
        _useCase = new GetParentOrganisationsUseCase(Mock.Of<ILogger<GetParentOrganisationsUseCase>>(), _mockHierarchyRepository.Object);
    }

    [Fact]
    public async Task Execute_WithEmptyGuid_ReturnsFailureResponse()
    {
        var result = await _useCase.Execute(Guid.Empty);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Empty(result.ParentOrganisations);
    }

    [Fact]
    public async Task Execute_WhenNoParents_ReturnsEmptyList()
    {
        var childId = Guid.NewGuid();
        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(childId))
            .ReturnsAsync(Array.Empty<OrganisationHierarchy>());

        var result = await _useCase.Execute(childId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.ParentOrganisations);
    }

    [Fact]
    public async Task Execute_WithValidParents_ReturnsParentOrganisations()
    {
        var childId = Guid.NewGuid();
        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();

        var hierarchies = new List<OrganisationHierarchy>
        {
            CreateHierarchy(parent1Id, "Parent 1", "GB-PPON:12345", childId),
            CreateHierarchy(parent2Id, "Parent 2", "GB-PPON:67890", childId)
        };

        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(childId))
            .ReturnsAsync(hierarchies);

        var result = await _useCase.Execute(childId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        var parentOrgs = result.ParentOrganisations.ToList();
        Assert.Equal(2, parentOrgs.Count);

        var parent1 = parentOrgs[0];
        Assert.Equal(parent1Id, parent1.Id);
        Assert.Equal("Parent 1", parent1.Name);
        Assert.Equal("GB-PPON:12345", parent1.Ppon);

        var parent2 = parentOrgs[1];
        Assert.Equal(parent2Id, parent2.Id);
        Assert.Equal("Parent 2", parent2.Name);
        Assert.Equal("GB-PPON:67890", parent2.Ppon);
    }

    [Fact]
    public async Task Execute_WithNonPponIdentifiers_ReturnEmptyPpon()
    {
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var hierarchy = CreateHierarchy(parentId, "Parent", "OTHER-SCHEME:12345", childId);

        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(childId))
            .ReturnsAsync(new[] { hierarchy });

        var result = await _useCase.Execute(childId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.ParentOrganisations);
        Assert.Equal(string.Empty, result.ParentOrganisations.First().Ppon);
    }

    [Fact]
    public async Task Execute_WithRepositoryException_ReturnsFailureResponse()
    {
        var childId = Guid.NewGuid();
        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(childId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _useCase.Execute(childId);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Empty(result.ParentOrganisations);
    }

    [Fact]
    public async Task Execute_WithNullParentInHierarchy_SkipsNullParent()
    {
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var hierarchies = new List<OrganisationHierarchy>
        {
            CreateHierarchyWithNullParent(),
            CreateHierarchy(parentId, "Valid Parent", "GB-PPON:12345", childId)
        };

        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(childId))
            .ReturnsAsync(hierarchies);

        var result = await _useCase.Execute(childId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.ParentOrganisations);
        Assert.Equal(parentId, result.ParentOrganisations.First().Id);
    }

    [Fact]
    public async Task Execute_WithMixedHierarchyData_ReturnsOnlyMatchingParents()
    {
        var targetChildId = Guid.NewGuid();
        var otherChildId = Guid.NewGuid();
        var validParentIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var hierarchies = new List<OrganisationHierarchy>
        {
            CreateHierarchy(validParentIds[0], "Valid Parent 1", "GB-PPON:VALID-1", targetChildId),
            CreateHierarchy(validParentIds[1], "Valid Parent 2", "GB-PPON:VALID-2", targetChildId),
            CreateHierarchy(validParentIds[2], "Valid Parent 3", "GB-PPON:VALID-3", targetChildId),

            CreateHierarchyWithNullParent(),
            CreateHierarchyWithNullParent(),

            CreateHierarchy(Guid.NewGuid(), "Other Parent 1", "GB-PPON:OTHER-1", otherChildId),
            CreateHierarchy(Guid.NewGuid(), "Other Parent 2", "GB-PPON:OTHER-2", otherChildId)
        };

        _mockHierarchyRepository
            .Setup(x => x.GetParentsAsync(targetChildId))
            .ReturnsAsync(hierarchies.Where(h => h.Child?.Guid == targetChildId));

        var result = await _useCase.Execute(targetChildId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        var parents = result.ParentOrganisations.ToList();

        Assert.Equal(3, parents.Count);

        for (var i = 0; i < validParentIds.Length; i++)
        {
            var parent = parents[i];
            Assert.Equal(validParentIds[i], parent.Id);
            Assert.Equal($"Valid Parent {i + 1}", parent.Name);
            Assert.Equal($"GB-PPON:VALID-{i + 1}", parent.Ppon);
        }

        Assert.All(parents, parent =>
            Assert.Contains(PartyRole.Buyer, parent.Roles));
    }

    private static OrganisationHierarchy CreateHierarchy(Guid parentId, string parentName, string identifierId, Guid childId)
    {
        var parent = CreateOrganisation(parentId, parentName, identifierId);
        var child = CreateOrganisation(childId, "Child Organisation", "GB-PPON:CHILD-123");

        return new OrganisationHierarchy
        {
            RelationshipId = Guid.NewGuid(),
            Parent = parent,
            ParentOrganisationId = parentId.GetHashCode(),
            Child = child,
            ChildOrganisationId = childId.GetHashCode()
        };
    }

    private static OrganisationHierarchy CreateHierarchyWithNullParent()
    {
        var childId = Guid.NewGuid();
        var child = CreateOrganisation(childId, "Child Organisation", "GB-PPON:CHILD-123");

        return new OrganisationHierarchy
        {
            RelationshipId = Guid.NewGuid(),
            Parent = null,
            ParentOrganisationId = 0,
            Child = child,
            ChildOrganisationId = childId.GetHashCode()
        };
    }

    private static OrganisationInformation.Persistence.Organisation CreateOrganisation(Guid id, string name, string identifierId)
    {
        return new OrganisationInformation.Persistence.Organisation
        {
            Guid = id,
            Name = name,
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "test" },
            Type = OrganisationType.Organisation,
            Roles = new List<PartyRole> { PartyRole.Buyer },
            Identifiers = new List<OrganisationInformation.Persistence.Identifier>
            {
                new()
                {
                    Scheme = identifierId.Split(':')[0],
                    LegalName = name,
                    Primary = true,
                    IdentifierId = identifierId
                }
            }
        };
    }
}