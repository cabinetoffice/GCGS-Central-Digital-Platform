using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseShareCodeRepository(OrganisationInformationContext context) : IShareCodeRepository
{
    public async Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Submitted && x.Organisation.Guid == organisationId)
            .OrderByDescending(y => y.SubmittedAt).ToListAsync();
    }

    public async Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Draft)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Organisation.Guid == organisationId);
    }

    public async Task<SharedConsent?> GetByShareCode(string sharecode)
    {
        return await context.SharedConsents
            .Where(sc => sc.BookingReference == sharecode)
            .Include(sc => sc.Organisation)
                .ThenInclude(o => o.Identifiers)
            .Include(sc => sc.Organisation)
                .ThenInclude(o => o.ContactPoints)
            .Include(sc => sc.Organisation)
                .ThenInclude(o => o.Addresses)
                    .ThenInclude(p => p.Address)
            .Include(sc => sc.Organisation)
                .ThenInclude(o => o.OrganisationPersons)
                    .ThenInclude(op => op.Person)
            .Include(sc => sc.AnswerSets)
                .ThenInclude(sa => sa.Answers)
            .Include(sc => sc.Form)
                .ThenInclude(f => f.Sections)
                    .ThenInclude(s => s.Questions)
            .FirstOrDefaultAsync();
    }


    public void Dispose()
    {
        context.Dispose();
    }

}
