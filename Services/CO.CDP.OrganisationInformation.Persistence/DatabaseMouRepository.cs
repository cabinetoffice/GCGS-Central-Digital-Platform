using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseMouRepository
    (OrganisationInformationContext context) : IMouRepository
{
    public async Task<IEnumerable<MouReminderOrganisation>> GetMouReminderOrganisations()
    {
        var time7daysAgo = DateTimeOffset.UtcNow.AddDays(-7);

        var query = from o in context.Organisations
                    from cp in o.ContactPoints
                    from op in o.OrganisationPersons
                    join p in context.Persons on op.PersonId equals p.Id

                    where o.Roles.Contains(PartyRole.Buyer)
                    where EF.Functions.JsonContains(op.Scopes, "[\"ADMIN\"]")

                    let latestMouId = context.Mou.OrderByDescending(m => m.CreatedOn).Select(m => m.Id).FirstOrDefault()
                    where !context.MouSignature.Any(ms => ms.OrganisationId == o.Id && ms.MouId == latestMouId)

                    let lastReminder = context.MouEmailReminders.Where(mer => mer.OrganisationId == o.Id).FirstOrDefault()
                    where lastReminder == null || lastReminder.ReminderSentOn <= time7daysAgo

                    group p by new { o.Id, o.Guid, o.Name, cp.Email } into g
                    select new MouReminderOrganisation
                    {
                        Id = g.Key.Id,
                        Guid = g.Key.Guid,
                        Name = g.Key.Name,
                        Email = g.Key.Email + "," + string.Join(",", g.Select(x => x.Email))
                    };

        var results = await query.ToListAsync();

        return results;
    }

    public async Task UpsertMouEmailReminder(int organisationId)
    {
        var existing = await context.MouEmailReminders
            .FirstOrDefaultAsync(r => r.OrganisationId == organisationId);

        if (existing != null)
        {
            existing.ReminderSentOn = DateTimeOffset.UtcNow;
        }
        else
        {
            await context.MouEmailReminders.AddAsync(
                new MouEmailReminder
                {
                    OrganisationId = organisationId,
                    ReminderSentOn = DateTimeOffset.UtcNow
                });
        }

        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}