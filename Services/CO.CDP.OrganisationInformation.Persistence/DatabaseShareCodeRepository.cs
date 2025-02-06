using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseShareCodeRepository(OrganisationInformationContext context) : IShareCodeRepository
{
    public async Task<bool> ShareCodeDocumentExistsAsync(string shareCode, string documentId)
    {
        return await context.Set<FormAnswer>()
            .Where(x => x.Question.Type == FormQuestionType.FileUpload
                && x.TextValue == documentId
                && x.FormAnswerSet!.Deleted == false
                && x.FormAnswerSet!.SharedConsent.ShareCode == shareCode)
            .AnyAsync();
    }

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
            .Where(sc => sc.ShareCode == sharecode)
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
            .Include(sc => sc.AnswerSets.Where(x => !x.Deleted && x.Section.Type != FormSectionType.Declaration))
                .ThenInclude(sa => sa.Section)
            .Include(sc => sc.AnswerSets.Where(x => !x.Deleted && x.Section.Type != FormSectionType.Declaration))
                .ThenInclude(sa => sa.Answers)
                    .ThenInclude(a => a.Question)
            .Include(sc => sc.Form)
                .ThenInclude(f => f.Sections)
                    .ThenInclude(s => s.Questions.OrderBy(q => q.SortOrder))
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

    public async Task<SharedConsentDetails?> GetShareCodeDetailsAsync(Guid organisationId, string shareCode)
    {
        var query = from s in context.SharedConsents
                    join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                    join fs in context.Set<FormSection>() on fas.SectionId equals fs.Id
                    join fa in context.Set<FormAnswer>() on fas.Id equals fa.FormAnswerSetId
                    join fq in context.Set<FormQuestion>() on fa.QuestionId equals fq.Id
                    join o in context.Organisations on s.OrganisationId equals o.Id
                    where
                        fs.Type == FormSectionType.Declaration
                        && fas.Deleted == false
                        && o.Guid == organisationId
                        && s.ShareCode == shareCode
                    select new
                    {
                        FormAnswerSetId = fas.Id,
                        FormAnswerSetUpdate = fas.UpdatedOn,
                        s.ShareCode,
                        s.SubmittedAt,
                        QuestionId = fq.Guid,
                        QuestionType = fq.Type,
                        fq.SummaryTitle,
                        fq.Title,
                        FormAnswer = fa
                    };

        var data = await query.ToListAsync();
        var sharedCodeResult = data.OrderByDescending(x => x.FormAnswerSetUpdate).GroupBy(g => new { g.ShareCode, g.FormAnswerSetId, g.SubmittedAt }).FirstOrDefault();
        if (sharedCodeResult == null) return null;

        return new SharedConsentDetails
        {
            ShareCode = sharedCodeResult.Key.ShareCode,
            SubmittedAt = sharedCodeResult.Key.SubmittedAt!.Value,
            QuestionAnswers = sharedCodeResult.Select(a =>
            new SharedConsentQuestionAnswer
            {
                QuestionId = a.QuestionId,
                QuestionType = a.QuestionType,
                Title = string.IsNullOrEmpty(a.SummaryTitle) ? a.Title : a.SummaryTitle,
                Answer = a.FormAnswer
            })
        };
    }

    public async Task<bool?> GetShareCodeVerifyAsync(string formVersionId, string shareCode)
    {
        // Get FormId and Organisation based on ShareCode and FormVersionId
        var query = from s in context.SharedConsents
                    where
                        s.FormVersionId == formVersionId
                        && s.ShareCode == shareCode
                    select s;

        if (query.Count() > 1) return false; // Scenario-1: 

        var data = await query.FirstOrDefaultAsync();
        if (data == null) return null; // Scenario-2: if Sharecode not found

        // Get the latest SharedConsent records of the Organistaion and FormVersionId and FormId
        var latestShareCode = await (from s in context.SharedConsents
                                     join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                                     where
                                         fas.Deleted == false
                                         && s.OrganisationId == data.OrganisationId
                                         && s.FormId == data.FormId
                                         && s.FormVersionId == data!.FormVersionId
                                     orderby s.UpdatedOn descending
                                     select s).Take(1).FirstOrDefaultAsync();

        if (latestShareCode!.SubmissionState != SubmissionState.Submitted) return false; // Scenario-3: Sharecode is not submitted

        if (data!.ShareCode == latestShareCode.ShareCode
            && data!.ShareCode == shareCode
                && data!.SubmissionState == SubmissionState.Submitted) return true; //Scenario-4: if requested sharecode is latest Sharecode and stae is submitted

        return false; //Scenario-4: if scenario-4 is not passed
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
