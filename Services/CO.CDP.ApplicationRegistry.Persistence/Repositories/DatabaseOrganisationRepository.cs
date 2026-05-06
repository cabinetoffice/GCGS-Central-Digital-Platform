using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseOrganisationRepository : IOrganisationRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseOrganisationRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<Organisation?> GetByIdAsync(Guid id)
    {
        return await _context.Organisations
            .Include(o => o.Members.Where(m => m.IsActive))
            .Include(o => o.Applications)
                .ThenInclude(oa => oa.Application)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organisation?> GetBySlugAsync(string slug)
    {
        return await _context.Organisations
            .Include(o => o.Members.Where(m => m.IsActive))
            .FirstOrDefaultAsync(o => o.Slug == slug);
    }

    public async Task<IEnumerable<Organisation>> GetAllAsync(string? name = null, string? type = null)
    {
        var query = _context.Organisations.Where(o => o.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(o => o.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(o => o.Type == type);
        }

        return await query.OrderBy(o => o.Name).ToListAsync();
    }

    public async Task<Organisation> CreateAsync(Organisation organisation)
    {
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();
        return organisation;
    }

    public async Task UpdateAsync(Organisation organisation)
    {
        organisation.UpdatedOn = DateTimeOffset.UtcNow;
        _context.Organisations.Update(organisation);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserOrganisationMembership>> GetMembersAsync(Guid organisationId)
    {
        return await _context.UserOrganisationMemberships
            .Where(m => m.OrganisationId == organisationId && m.IsActive)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task<UserOrganisationMembership?> GetMemberAsync(Guid organisationId, string userPrincipalId)
    {
        return await _context.UserOrganisationMemberships
            .FirstOrDefaultAsync(m => m.OrganisationId == organisationId
                && m.UserPrincipalId == userPrincipalId
                && m.IsActive);
    }

    public async Task AddMemberAsync(UserOrganisationMembership membership)
    {
        _context.UserOrganisationMemberships.Add(membership);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMemberAsync(UserOrganisationMembership membership)
    {
        _context.UserOrganisationMemberships.Update(membership);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrganisationApplication>> GetOrganisationApplicationsAsync(Guid organisationId)
    {
        return await _context.OrganisationApplications
            .Include(oa => oa.Application)
            .Where(oa => oa.OrganisationId == organisationId && oa.Application.IsActive)
            .ToListAsync();
    }

    public async Task EnableApplicationAsync(OrganisationApplication organisationApplication)
    {
        _context.OrganisationApplications.Add(organisationApplication);
        await _context.SaveChangesAsync();
    }

    public async Task DisableApplicationAsync(Guid organisationId, Guid applicationId)
    {
        var entry = await _context.OrganisationApplications
            .FirstOrDefaultAsync(oa => oa.OrganisationId == organisationId && oa.ApplicationId == applicationId);

        if (entry != null)
        {
            _context.OrganisationApplications.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
