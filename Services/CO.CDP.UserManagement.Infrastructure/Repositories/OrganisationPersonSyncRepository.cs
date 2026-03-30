using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

public class OrganisationPersonSyncRepository(OrganisationInformationContext context) : IOrganisationPersonSyncRepository
{
    private static readonly IReadOnlySet<string> UmManagedScopes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            OrganisationPersonScopes.Admin,
            OrganisationPersonScopes.Editor,
            OrganisationPersonScopes.Viewer,
            OrganisationPersonScopes.Responder,
        };

    public async Task UpsertAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        IReadOnlyList<string> computedScopes,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindAsync(cdpOrganisationGuid, cdpPersonGuid, cancellationToken);

        var merged = MergeScopes(existing?.Scopes ?? [], computedScopes);

        if (existing != null)
        {
            existing.Scopes = merged;
            context.Update(existing);
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var org = await context.Organisations
            .FirstOrDefaultAsync(o => o.Guid == cdpOrganisationGuid, cancellationToken);
        var person = await context.Persons
            .FirstOrDefaultAsync(p => p.Guid == cdpPersonGuid, cancellationToken);

        if (org == null || person == null) return;

        context.Add(new OrganisationPerson
        {
            Organisation = org,
            Person = person,
            Scopes = merged,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindAsync(cdpOrganisationGuid, cdpPersonGuid, cancellationToken);

        if (existing == null) return;

        var externalScopes = ExternalScopes(existing.Scopes);

        if (externalScopes.Count > 0)
        {
            existing.Scopes = externalScopes;
            context.Update(existing);
        }
        else
        {
            context.Remove(existing);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private Task<OrganisationPerson?> FindAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        CancellationToken ct) =>
        context.Set<OrganisationPerson>()
            .Include(op => op.Organisation)
            .Include(op => op.Person)
            .FirstOrDefaultAsync(
                op => op.Organisation != null
                   && op.Organisation.Guid == cdpOrganisationGuid
                   && op.Person.Guid == cdpPersonGuid,
                ct);

    private static List<string> MergeScopes(
        IReadOnlyList<string> existing,
        IReadOnlyList<string> computed) =>
        ExternalScopes(existing)
            .Concat(computed)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static List<string> ExternalScopes(IReadOnlyList<string> scopes) =>
        scopes.Where(s => !UmManagedScopes.Contains(s)).ToList();
}
