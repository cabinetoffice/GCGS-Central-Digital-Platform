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
        public async Task<Guid> CreateRelationshipAsync(Guid parentId, Guid childId, List<PartyRole> roles)
        {
            if (parentId == Guid.Empty)
                throw new ArgumentException("Parent ID cannot be empty", nameof(parentId));

            if (childId == Guid.Empty)
                throw new ArgumentException("Child ID cannot be empty", nameof(childId));

            if (roles == null || !roles.Any())
                throw new ArgumentException("At least one role must be specified", nameof(roles));

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

            var existingRelationship = await _context.OrganisationHierarchies
                .Where(h => h.ParentOrganisationId == parent.Id &&
                            h.ChildOrganisationId == child.Id &&
                            h.SupersededOn == null)
                .FirstOrDefaultAsync();

            if (existingRelationship != null)
            {
                existingRelationship.Roles = roles;
                await _context.SaveChangesAsync();
                return existingRelationship.RelationshipId;
            }

            var relationshipId = Guid.NewGuid();
            var hierarchy = new OrganisationHierarchy
            {
                RelationshipId = relationshipId,
                ParentOrganisationId = parent.Id,
                ChildOrganisationId = child.Id,
                Roles = roles,
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

            // Get the parent organisation to retrieve its database ID
            var parent = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Guid == parentId);

            if (parent == null)
                return Enumerable.Empty<OrganisationHierarchy>();

            return await _context.OrganisationHierarchies
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
    }
}
