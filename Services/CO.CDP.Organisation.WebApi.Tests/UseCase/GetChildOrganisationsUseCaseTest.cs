using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetChildOrganisationsUseCaseTest
{
    private readonly Mock<ILogger<GetChildOrganisationsUseCase>> _mockLogger;
    private readonly Mock<IOrganisationHierarchyRepository> _mockHierarchyRepository;
    private readonly IGetChildOrganisationsUseCase _useCase;

    public GetChildOrganisationsUseCaseTest()
    {
        _mockLogger = new Mock<ILogger<GetChildOrganisationsUseCase>>();
        _mockHierarchyRepository = new Mock<IOrganisationHierarchyRepository>();
        _useCase = new GetChildOrganisationsUseCase(_mockLogger.Object, _mockHierarchyRepository.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("logger",
            () => new GetChildOrganisationsUseCase(null!, _mockHierarchyRepository.Object));
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("hierarchyRepository",
            () => new GetChildOrganisationsUseCase(_mockLogger.Object, null!));
    }

    [Fact]
    public async Task Execute_WithEmptyGuid_ReturnsFailureResponse()
    {
        var emptyParentId = Guid.Empty;

        var response = await _useCase.Execute(emptyParentId);

        Assert.False(response.Success);
        Assert.Empty(response.ChildOrganisations);
    }

    [Fact]
    public async Task Execute_WithValidGuidButNoChildren_ReturnsSuccessWithEmptyCollection()
    {
        var parentId = Guid.NewGuid();
        _mockHierarchyRepository
            .Setup(repo => repo.GetChildrenAsync(parentId))
            .ReturnsAsync(Array.Empty<OrganisationHierarchy>());

        var response = await _useCase.Execute(parentId);

        Assert.True(response.Success);
        Assert.Empty(response.ChildOrganisations);
        _mockHierarchyRepository.Verify(repo => repo.GetChildrenAsync(parentId), Times.Once);
    }

    [Fact]
    public async Task Execute_WithChildrenOrganisations_ReturnsSuccessWithMappedChildren()
    {
        var parentId = Guid.NewGuid();
        var childOrgs = CreateTestOrganisationHierarchies(1);

        _mockHierarchyRepository
            .Setup(repo => repo.GetChildrenAsync(parentId))
            .ReturnsAsync(childOrgs);

        var response = await _useCase.Execute(parentId);

        Assert.True(response.Success);
        Assert.Equal(2, response.ChildOrganisations.Count());

        var firstChild = response.ChildOrganisations.First();
        Assert.Equal(childOrgs[0].Child!.Guid, firstChild.Id);
        Assert.Equal(childOrgs[0].Child!.Name, firstChild.Name);
        Assert.Equal("PPON123", firstChild.Identifier);

        var secondChild = response.ChildOrganisations.ElementAt(1);
        Assert.Equal(childOrgs[1].Child!.Guid, secondChild.Id);
        Assert.Equal(childOrgs[1].Child!.Name, secondChild.Name);
        Assert.Equal(string.Empty, secondChild.Identifier);

        _mockHierarchyRepository.Verify(repo => repo.GetChildrenAsync(parentId), Times.Once);
    }

    [Fact]
    public async Task Execute_WithException_ReturnsFailureResponse()
    {
        var parentId = Guid.NewGuid();
        _mockHierarchyRepository
            .Setup(repo => repo.GetChildrenAsync(parentId))
            .ThrowsAsync(new Exception("Test exception"));

        var response = await _useCase.Execute(parentId);

        Assert.False(response.Success);
        Assert.Empty(response.ChildOrganisations);
        _mockHierarchyRepository.Verify(repo => repo.GetChildrenAsync(parentId), Times.Once);
    }

    [Fact]
    public async Task Execute_WithNullChildInHierarchy_SkipsNullChild()
    {
        var parentId = Guid.NewGuid();
        var childGuid = Guid.NewGuid();
        var hierarchies = new List<OrganisationHierarchy>
        {
            new()
            {
                RelationshipId = Guid.NewGuid(),
                ParentOrganisationId = 1,
                ChildOrganisationId = 2,
                Child = new OrganisationInformation.Persistence.Organisation
                {
                    Id = 2,
                    Guid = childGuid,
                    Name = "Valid Child",
                    Identifiers = new List<Identifier>(),
                    Tenant = new Tenant
                    {
                        Guid = Guid.NewGuid(),
                        Name = "Tenant"
                    },
                    Type = OrganisationType.Organisation
                }
            },
            new()
            {
                RelationshipId = Guid.NewGuid(),
                ParentOrganisationId = 1,
                ChildOrganisationId = 3,
                Child = null
            }
        };

        _mockHierarchyRepository
            .Setup(repo => repo.GetChildrenAsync(parentId))
            .ReturnsAsync(hierarchies);

        var response = await _useCase.Execute(parentId);

        Assert.True(response.Success);
        Assert.Single(response.ChildOrganisations);
        Assert.Equal("Valid Child", response.ChildOrganisations.First().Name);
    }

    [Fact]
    public async Task Execute_WithMultiplePponIdentifiers_UsesFirstMatch()
    {
        var parentId = Guid.NewGuid();
        var childGuid = Guid.NewGuid();

        var identifiers = new List<Identifier>
        {
            new()
            {
                Scheme = "Some-Other-Scheme", IdentifierId = "OTHER123", LegalName = "Legal Name", Primary = false
            },
            new() { Scheme = "GB-PPON", IdentifierId = "PPON-FIRST", LegalName = "Legal Name", Primary = true },
            new()
            {
                Scheme = "gb-ppon", IdentifierId = "PPON-SECOND", LegalName = "Legal Name", Primary = false
            }
        };

        var hierarchies = new List<OrganisationHierarchy>
        {
            new()
            {
                RelationshipId = Guid.NewGuid(),
                ParentOrganisationId = 1,
                ChildOrganisationId = 2,
                Child = new OrganisationInformation.Persistence.Organisation
                {
                    Id = 2,
                    Guid = childGuid,
                    Name = "Child With Multiple PPONs",
                    Identifiers = identifiers,
                    Tenant = new Tenant
                    {
                        Guid = Guid.NewGuid(),
                        Name = "Tenant"
                    },
                    Type = OrganisationType.Organisation
                }
            }
        };

        _mockHierarchyRepository
            .Setup(repo => repo.GetChildrenAsync(parentId))
            .ReturnsAsync(hierarchies);

        var response = await _useCase.Execute(parentId);

        Assert.True(response.Success);
        Assert.Single(response.ChildOrganisations);
        Assert.Equal("PPON-FIRST", response.ChildOrganisations.First().Identifier);
    }

    private static List<OrganisationHierarchy> CreateTestOrganisationHierarchies(int parentId)
    {
        var childOrg1 = new OrganisationInformation.Persistence.Organisation
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            Name = "Child Org 1",
            Identifiers = new List<Identifier>
            {
                new() { Scheme = "GB-PPON", IdentifierId = "PPON123", LegalName = "Legal Name 1", Primary = true }
            },
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 1"
            },
            Type = OrganisationType.Organisation
        };

        var childOrg2 = new OrganisationInformation.Persistence.Organisation
        {
            Id = 3,
            Guid = Guid.NewGuid(),
            Name = "Child Org 2",
            Identifiers = new List<Identifier>(),
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 2"
            },
            Type = OrganisationType.Organisation
        };

        return new List<OrganisationHierarchy>
        {
            new()
            {
                RelationshipId = Guid.NewGuid(),
                ParentOrganisationId = parentId,
                ChildOrganisationId = childOrg1.Id,
                Child = childOrg1
            },
            new()
            {
                RelationshipId = Guid.NewGuid(),
                ParentOrganisationId = parentId,
                ChildOrganisationId = childOrg2.Id,
                Child = childOrg2
            }
        };
    }
}