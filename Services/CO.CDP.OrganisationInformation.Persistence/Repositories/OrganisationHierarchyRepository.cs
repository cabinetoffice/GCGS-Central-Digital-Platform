using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence.Repositories
{
    /// <summary>
    /// Implementation of the organisation hierarchy repository
    /// </summary>
    public class OrganisationHierarchyRepository : IOrganisationHierarchyRepository
    {
        private readonly OrganisationInformationContext _context;

        /// <summary>
        /// Initialises a new instance of the OrganisationHierarchyRepository class
        /// </summary>
        /// <param name="context">The organisation information context</param>
        public OrganisationHierarchyRepository(OrganisationInformationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<Guid> CreateRelationshipAsync(Guid parentId, Guid childId)
        {
            if (parentId == Guid.Empty)
                throw new ArgumentException("Parent ID cannot be empty", nameof(parentId));

            if (childId == Guid.Empty)
                throw new ArgumentException("Child ID cannot be empty", nameof(childId));

            if (parentId == childId)
                throw new ArgumentException("Parent and child organisations cannot be the same");

            var parent = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Guid == parentId);

            if (parent == null)
                throw new ArgumentException($"Parent organisation with ID {parentId} does not exist", nameof(parentId));

            var child = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Guid == childId);

            if (child == null)
                throw new ArgumentException($"Child organisation with ID {childId} does not exist", nameof(childId));

            var inverseRelationship = await _context.OrganisationHierarchies
                .Where(h => h.ParentOrganisationId == child.Id &&
                            h.ChildOrganisationId == parent.Id &&
                            h.SupersededOn == null)
                .FirstOrDefaultAsync();

            if (inverseRelationship != null)
                throw new ArgumentException($"Child organisation with ID {childId} is already a parent to the parent organisation with ID {parentId}");

            var existingRelationship = await _context.OrganisationHierarchies
                .Where(h => h.ParentOrganisationId == parent.Id &&
                            h.ChildOrganisationId == child.Id &&
                            h.SupersededOn == null)
                .FirstOrDefaultAsync();

            if (existingRelationship != null)
            {
                return existingRelationship.RelationshipId;
            }

            var relationshipId = Guid.NewGuid();
            var hierarchy = new OrganisationHierarchy
            {
                RelationshipId = relationshipId,
                ParentOrganisationId = parent.Id,
                ChildOrganisationId = child.Id,
                CreatedOn = DateTime.UtcNow
            };

            await _context.OrganisationHierarchies.AddAsync(hierarchy);
            await _context.SaveChangesAsync();

            return relationshipId;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrganisationHierarchy>> GetChildrenAsync(Guid parentId)
        {
            if (parentId == Guid.Empty)
                throw new ArgumentException("Parent ID cannot be empty", nameof(parentId));

            var parent = await _context.Organisations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Guid == parentId);

            if (parent == null)
                return [];

            return await _context.OrganisationHierarchies
                .Include(h => h.Child)
                    .ThenInclude(c => c!.Identifiers)
                .Where(h => h.ParentOrganisationId == parent.Id && h.SupersededOn == null)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> SupersedeRelationshipAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID cannot be empty", nameof(id));

            var hierarchy = await _context.OrganisationHierarchies
                .FirstOrDefaultAsync(h => h.RelationshipId == id);

            if (hierarchy == null)
                return false;

            hierarchy.SupersededOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrganisationHierarchy>> GetParentsAsync(Guid childId)
        {
            if (childId == Guid.Empty)
                throw new ArgumentException("Child ID cannot be empty", nameof(childId));

            var child = await _context.Organisations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Guid == childId);

            if (child == null)
                return [];

            return await _context.OrganisationHierarchies
                .Include(h => h.Parent)
                .ThenInclude(p => p!.Identifiers)
                .Where(h => h.ChildOrganisationId == child.Id && h.SupersededOn == null)
                .ToListAsync();
        }
    }
}
