using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseFormRepository(OrganisationInformationContext context) : IFormRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    private struct FormSectionGroupSelection
    {
        public int? FormId { get; set; }

        public bool? FurtherQuestionsExempted { get; set; }
    }

    public async Task<IEnumerable<FormSectionSummary>> GetFormSummaryAsync(Guid formId, Guid organisationId)
    {
        var answersQuery = from sc in context.SharedConsents
                           join fas in context.FormAnswerSets on sc.Id equals fas.SharedConsentId
                           join o in context.Organisations on sc.OrganisationId equals o.Id
                           where o.Guid == organisationId && fas.Deleted == false
                           select new { sc.FormId, fas.SectionId, fas.FurtherQuestionsExempted, SharedConsentId = sc.Id };

        var currentSharedConsent = await context.SharedConsents
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync(x => x.Form.Guid == formId && x.Organisation.Guid == organisationId);

        if (currentSharedConsent != null)
        {
            answersQuery = answersQuery.Where(a => a.SharedConsentId == currentSharedConsent.Id);
        }

        var query = from f in context.Forms
                    join fss in context.Set<FormSection>() on f.Id equals fss.FormId
                    join subQuery in answersQuery on new { FormId = f.Id, SectionId = fss.Id } equals new { subQuery.FormId, subQuery.SectionId } into answers
                    from answer in answers.DefaultIfEmpty()
                    where f.Guid == formId
                    group new FormSectionGroupSelection { FormId = answer.FormId, FurtherQuestionsExempted = answer.FurtherQuestionsExempted }
                        by new { fss.Guid, fss.Title, fss.Type, fss.AllowsMultipleAnswerSets, fss.DisplayOrder } into g
                    orderby g.Key.DisplayOrder
                    select new FormSectionSummary
                    {
                        SectionId = g.Key.Guid,
                        SectionName = g.Key.Title,
                        Type = g.Key.Type,
                        AllowsMultipleAnswerSets = g.Key.AllowsMultipleAnswerSets,
                        AnswerSetCount = g.Count(a => a.FormId != null),
                        AnswerSetWithFurtherQuestionExemptedExists = g.Any(a => a.FurtherQuestionsExempted == true)
                    };

        return await query.ToListAsync();
    }

    public async Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId)
    {
        return await context.Set<FormSection>()
            .Include(s => s.Questions)
            .Include(f => f.Form)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Guid == sectionId);
    }

    public async Task SaveSharedConsentAsync(SharedConsent sharedConsent)
    {
        context.Update(sharedConsent);
        await context.SaveChangesAsync();
    }

    public async Task<SharedConsent?> GetSharedConsentWithAnswersAsync(Guid formId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Include(c => c.AnswerSets)
                .ThenInclude(a => a.Answers)
            .Include(c => c.AnswerSets)
                .ThenInclude(a => a.Section)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync(x => x.Form.Guid == formId && x.Organisation.Guid == organisationId);
    }

    public async Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId)
    {
        return await context.Set<FormQuestion>().Where(q => q.Section.Guid == sectionId).ToListAsync();
    }

    public async Task<List<FormAnswerSet>> GetFormAnswerSetsFromCurrentSharedConsentAsync(Guid sectionId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.Organisation.Guid == organisationId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(1)
            .Include(x => x.AnswerSets)
                .ThenInclude(a => a.Answers)
            .SelectMany(x => x.AnswerSets.Where(x => x.Section.Guid == sectionId && !x.Deleted))
            .ToListAsync();
    }

    public async Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId)
    {
        var answerSet = await context.Set<FormAnswerSet>()
            .Include(a => a.SharedConsent)
            .ThenInclude(s => s.Form)
            .Include(a => a.Section)
            .FirstOrDefaultAsync(a => a.SharedConsent.Organisation.Guid == organisationId && a.Guid == answerSetId);

        if (answerSet == null) return false;

        var sharedConsent = await GetSharedConsentWithAnswersAsync(answerSet.SharedConsent.Form.Guid, organisationId);

        if (sharedConsent == null) return false;

        sharedConsent = SharedConsentMapper.Map(sharedConsent);

        var sharedConsentAnswerSet = sharedConsent.AnswerSets
            .FirstOrDefault(a => a.Guid == answerSetId || a.CreatedFrom == answerSetId);

        if (sharedConsentAnswerSet == null) return false;

        sharedConsentAnswerSet.Deleted = true;
        context.Update(sharedConsent);
        await context.SaveChangesAsync();
        return true;
    }
}