using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using CO.CDP.OrganisationInformation.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Tests.Repositories
{
    public class OrganisationHierarchyRepositoryTests : IClassFixture<OrganisationInformationPostgreSqlFixture>
    {
        private readonly OrganisationInformationPostgreSqlFixture _fixture;
        private readonly IOrganisationHierarchyRepository _repository;
        private readonly OrganisationInformationContext _context;

        public OrganisationHierarchyRepositoryTests(OrganisationInformationPostgreSqlFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _context = _fixture.OrganisationInformationContext();
            _repository = new OrganisationHierarchyRepository(_context);

            CleanDatabase().Wait();
        }

        #region Test Helpers

        private async Task CleanDatabase()
        {
            _context.OrganisationHierarchies.RemoveRange(_context.OrganisationHierarchies);
            await _context.SaveChangesAsync();
        }

        private async Task<(Guid ParentId, Guid ChildId)> CreateTestOrganisations()
        {
            var tenant = await GetOrCreateTestTenant();

            var parentOrg = new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Parent Org {Guid.NewGuid()}",
                Tenant = tenant,
                Type = OrganisationType.Organisation,
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            };

            var childOrg = new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Child Org {Guid.NewGuid()}",
                Tenant = tenant,
                Type = OrganisationType.Organisation,
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            };

            _context.Organisations.Add(parentOrg);
            _context.Organisations.Add(childOrg);
            await _context.SaveChangesAsync();

            return (parentOrg.Guid, childOrg.Guid);
        }

        private async Task<Tenant> GetOrCreateTestTenant()
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == "Gov");

            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Guid = Guid.NewGuid(),
                    Name = "Gov",
                    CreatedOn = DateTimeOffset.UtcNow,
                    UpdatedOn = DateTimeOffset.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
            }

            return tenant;
        }

        private async Task<Guid> CreateTestHierarchy(Guid parentId, Guid childId)
        {
            var parent = await _context.Organisations.FirstAsync(o => o.Guid == parentId);
            var child = await _context.Organisations.FirstAsync(o => o.Guid == childId);

            var relationshipId = Guid.NewGuid();
            var hierarchy = new OrganisationHierarchy
            {
                RelationshipId = relationshipId,
                ParentOrganisationId = parent.Id,
                ChildOrganisationId = child.Id,
                CreatedOn = DateTime.UtcNow
            };

            _context.OrganisationHierarchies.Add(hierarchy);
            await _context.SaveChangesAsync();

            return relationshipId;
        }

        #endregion

        #region CreateRelationshipAsync Tests

        [Fact]
        public async Task CreateRelationshipAsync_WithValidData_ShouldCreateNewRelationship()
        {
            var (parentId, childId) = await CreateTestOrganisations();

            var relationshipId = await _repository.CreateRelationshipAsync(parentId, childId);

            relationshipId.Should().NotBeEmpty();

            var savedHierarchy = await _context.OrganisationHierarchies
                .FirstOrDefaultAsync(h => h.RelationshipId == relationshipId);

            savedHierarchy.Should().NotBeNull();

            var parent = await _context.Organisations.FirstAsync(o => o.Guid == parentId);
            var child = await _context.Organisations.FirstAsync(o => o.Guid == childId);

            savedHierarchy!.ParentOrganisationId.Should().Be(parent.Id);
            savedHierarchy.ChildOrganisationId.Should().Be(child.Id);
            savedHierarchy.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            savedHierarchy.SupersededOn.Should().BeNull();
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithExistingRelationship_ShouldReturnExistingRelationshipId()
        {
            var (parentId, childId) = await CreateTestOrganisations();

            var initialRelationshipId = await _repository.CreateRelationshipAsync(parentId, childId);

            var updatedRelationshipId = await _repository.CreateRelationshipAsync(parentId, childId);

            updatedRelationshipId.Should().Be(initialRelationshipId, "Should return the same relationship ID");

            var count = await _context.OrganisationHierarchies
                .Include(h => h.Parent)
                .Include(h => h.Child)
                .Where(h => h.Parent != null && h.Parent.Guid == parentId &&
                            h.Child != null && h.Child.Guid == childId)
                .CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithEmptyParentId_ShouldThrowArgumentException()
        {
            var (_, childId) = await CreateTestOrganisations();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(Guid.Empty, childId));

            exception.Message.Should().Contain("Parent ID cannot be empty");
            exception.ParamName.Should().Be("parentId");
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithEmptyChildId_ShouldThrowArgumentException()
        {
            var (parentId, _) = await CreateTestOrganisations();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(parentId, Guid.Empty));

            exception.Message.Should().Contain("Child ID cannot be empty");
            exception.ParamName.Should().Be("childId");
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithSameParentAndChild_ShouldThrowArgumentException()
        {
            var (parentId, _) = await CreateTestOrganisations();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(parentId, parentId));

            exception.Message.Should().Contain("Parent and child organisations cannot be the same");
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithNonExistentParent_ShouldThrowArgumentException()
        {
            var (_, childId) = await CreateTestOrganisations();
            var nonExistentParentId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(nonExistentParentId, childId));

            exception.Message.Should().Contain("Parent organisation with ID");
            exception.ParamName.Should().Be("parentId");
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithNonExistentChild_ShouldThrowArgumentException()
        {
            var (parentId, _) = await CreateTestOrganisations();
            var nonExistentChildId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(parentId, nonExistentChildId));

            exception.Message.Should().Contain("Child organisation with ID");
            exception.ParamName.Should().Be("childId");
        }

        [Fact]
        public async Task CreateRelationshipAsync_WithExistingInvertedChildToParentRelationship_ShouldThrowArgumentException()
        {
            var (parentId, childId) = await CreateTestOrganisations();

            var invertedRelationshipId = await _repository.CreateRelationshipAsync(childId, parentId);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.CreateRelationshipAsync(parentId, childId));

            exception.Message.Should()
                .Contain($"Child organisation with ID {childId} is already a parent to the parent organisation with ID {parentId}");
        }

        #endregion

        #region GetChildrenAsync Tests

        [Fact]
        public async Task GetChildrenAsync_WithExistingChildren_ShouldReturnActiveRelationships()
        {
            var (parentId, childId1) = await CreateTestOrganisations();

            var childOrg2 = new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Child Org 2 {Guid.NewGuid()}",
                Tenant = await GetOrCreateTestTenant(),
                Type = OrganisationType.Organisation,
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            };
            _context.Organisations.Add(childOrg2);
            await _context.SaveChangesAsync();
            var childId2 = childOrg2.Guid;

            var childOrg3 = new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Child Org 3 {Guid.NewGuid()}",
                Tenant = await GetOrCreateTestTenant(),
                Type = OrganisationType.Organisation,
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            };
            _context.Organisations.Add(childOrg3);
            await _context.SaveChangesAsync();
            var childId3 = childOrg3.Guid;

            await CreateTestHierarchy(parentId, childId1);
            await CreateTestHierarchy(parentId, childId2);

            var supersededRelationshipId = await CreateTestHierarchy(
                parentId,
                childId3
            );

            var supersededRelationship = await _context.OrganisationHierarchies
                .FirstAsync(h => h.RelationshipId == supersededRelationshipId);
            supersededRelationship.SupersededOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var children = await _repository.GetChildrenAsync(parentId);

            children.Should().NotBeNull();
            children.Should().HaveCount(2);

            var childrenWithNavProperties = await _context.OrganisationHierarchies
                .Where(h => children.Select(c => c.Id).Contains(h.Id))
                .Include(h => h.Child)
                .ToListAsync();

            childrenWithNavProperties.Should().Contain(h => h.Child != null && h.Child.Guid == childId1);
            childrenWithNavProperties.Should().Contain(h => h.Child != null && h.Child.Guid == childId2);
            children.Should().NotContain(h => h.SupersededOn != null);
        }

        [Fact]
        public async Task GetChildrenAsync_WithNoChildren_ShouldReturnEmptyCollection()
        {
            var (parentId, _) = await CreateTestOrganisations();

            var children = await _repository.GetChildrenAsync(parentId);

            children.Should().NotBeNull();
            children.Should().BeEmpty();
        }

        [Fact]
        public async Task GetChildrenAsync_WithEmptyParentId_ShouldThrowArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetChildrenAsync(Guid.Empty));

            exception.Message.Should().Contain("Parent ID cannot be empty");
            exception.ParamName.Should().Be("parentId");
        }

        #endregion

        #region SupersedeRelationshipAsync Tests

        [Fact]
        public async Task SupersedeRelationshipAsync_WithExistingRelationship_ShouldMarkAsSuperseded()
        {
            var (parentId, childId) = await CreateTestOrganisations();
            var relationshipId = await CreateTestHierarchy(parentId, childId);

            var result = await _repository.SupersedeRelationshipAsync(relationshipId);

            result.Should().BeTrue();

            var supersededHierarchy = await _context.OrganisationHierarchies
                .FirstOrDefaultAsync(h => h.RelationshipId == relationshipId);

            supersededHierarchy.Should().NotBeNull();
            supersededHierarchy!.SupersededOn.Should().NotBeNull();
            supersededHierarchy.SupersededOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task SupersedeRelationshipAsync_WithNonExistentRelationship_ShouldReturnFalse()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _repository.SupersedeRelationshipAsync(nonExistentId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SupersedeRelationshipAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(() => _repository.SupersedeRelationshipAsync(Guid.Empty));

            exception.Message.Should().Contain("ID cannot be empty");
            exception.ParamName.Should().Be("id");
        }

        #endregion
    }
}