using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
    }

    public async Task<IEnumerable<FormSectionSummary>> GetFormSummaryAsync(Guid formId, Guid organisationId)
    {
        var answersQuery = from sc in context.SharedConsents
                           join fas in context.FormAnswerSets on sc.Id equals fas.SharedConsentId
                           join o in context.Organisations on sc.OrganisationId equals o.Id
                           where o.Guid == organisationId && fas.Deleted == false
                           select new { sc.FormId, fas.SectionId };

        var query = from f in context.Forms
                    join fss in context.Set<FormSection>() on f.Id equals fss.FormId
                    join subQuery in answersQuery on new { FormId = f.Id, SectionId = fss.Id } equals new { subQuery.FormId, subQuery.SectionId } into answers
                    from answer in answers.DefaultIfEmpty()
                    where f.Guid == formId
                    group new FormSectionGroupSelection { FormId = answer.FormId } by new { fss.Guid, fss.Title, fss.Type, fss.AllowsMultipleAnswerSets } into g
                    select new FormSectionSummary
                    {
                        SectionId = g.Key.Guid,
                        SectionName = g.Key.Title,
                        Type = g.Key.Type,
                        AllowsMultipleAnswerSets = g.Key.AllowsMultipleAnswerSets,
                        AnswerSetCount = g.Count(a => a.FormId != null)
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

    public async Task SaveFormAsync(Form form)
    {
        context.Update(form);
        await context.SaveChangesAsync();
    }

    public async Task SaveSharedConsentAsync(SharedConsent sharedConsent)
    {
        context.Update(sharedConsent);
        await context.SaveChangesAsync();
    }

    public async Task<FormSection?> GetFormSectionAsync(Guid sectionId)
    {
        return await context.Set<FormSection>()
            .FirstOrDefaultAsync(s => s.Guid == sectionId);
    }

    public async Task<SharedConsent?> GetSharedConsentDraftWithAnswersAsync(Guid formId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Include(c => c.AnswerSets)
            .ThenInclude(a => a.Answers)
            .Where(x => x.SubmissionState == SubmissionState.Draft)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Organisation.Guid == organisationId);
    }

    public async Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Submitted && x.Organisation.Guid == organisationId)
            .OrderByDescending(y => y.SubmittedAt).ToListAsync();
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
                Title = a.SummaryTitle,
                Answer = a.FormAnswer
            })
        };
    }

    public async Task<Boolean?> GetShareCodeVerifyAsync(string formVersionId, string shareCode)
    {
        var query = from s in context.SharedConsents
                    join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                    where
                        fas.Deleted == false
                        && s.FormVersionId == formVersionId
                        && s.ShareCode == shareCode
                    select s;

        if (query.Count() > 1) return false; // Scenario-1: if one sharecode with multiple answersets

        var data = await query.FirstOrDefaultAsync();
        if (data == null) return null; // Scenario-2: if Sharecode not found

        // Get the latest SharedConsent records of the Organistaion and FormVersionId
        var latestShareCode = await (from s in context.SharedConsents
                                     join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                                     where
                                         fas.Deleted == false
                                         && s.FormVersionId == data!.FormVersionId
                                         && s.OrganisationId == data.OrganisationId
                                     orderby s.UpdatedOn descending
                                     select s).Take(1).FirstOrDefaultAsync();


        if (latestShareCode!.SubmissionState != SubmissionState.Submitted) return false; // Scenario-3: Sharecode is not submitted

        if (data!.ShareCode == latestShareCode.ShareCode
            && data!.ShareCode == shareCode
                && data!.SubmissionState == SubmissionState.Submitted) return true; //Scenario-4: if requested sharecode is latest Sharecode and stae is submitted

        return false; //Scenario-4: if scenario-4 is not passed
    }

    public async Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId)
    {
        return await context.Set<FormQuestion>().Where(q => q.Section.Guid == sectionId).ToListAsync();
    }

    public async Task<List<FormAnswerSet>> GetFormAnswerSetsAsync(Guid sectionId, Guid organisationId)
    {
        return await context.Set<FormAnswerSet>()
            .Include(a => a.Answers)
            .Include(a => a.SharedConsent)
            .Where(a => a.Section.Guid == sectionId && a.SharedConsent.Organisation.Guid == organisationId && a.Deleted == false)
            .ToListAsync();
    }

    public async Task<FormAnswerSet?> GetFormAnswerSetAsync(Guid sectionId, Guid organisationId, Guid answerSetId)
    {
        return await context.Set<FormAnswerSet>()
            .Include(a => a.Answers)
            .Include(b => b.SharedConsent)
            .FirstOrDefaultAsync(a => a.Guid == answerSetId && a.Section.Guid == sectionId && a.SharedConsent.Organisation.Guid == organisationId);
    }

    public async Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId)
    {
        var answerSet = await context.Set<FormAnswerSet>()
            .Include(a => a.SharedConsent)
            .FirstOrDefaultAsync(a => a.SharedConsent.Organisation.Guid == organisationId && a.Guid == answerSetId);

        if (answerSet == null) return false;

        answerSet.Deleted = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task SaveAnswerSet(FormAnswerSet answerSet)
    {
        var existingAnswerSet = await context.Set<FormAnswerSet>().FirstOrDefaultAsync(a => a.Guid == answerSet.Guid);

        if (existingAnswerSet != null)
        {
            context.Entry(existingAnswerSet).CurrentValues.SetValues(answerSet);
            context.Entry(existingAnswerSet).State = EntityState.Modified;
        }
        else
        {
            context.Add(answerSet);
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            HandleDbUpdateException(answerSet, ex);
        }
    }

    private static void HandleDbUpdateException(FormAnswerSet answerSet, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.Message.Contains("_form_answer_sets_guid"):
                throw new IConnectedEntityRepository.ConnectedEntityRepositoryException.DuplicateConnectedEntityException(
                    $"Form answer set with guid `{answerSet.Guid}` already exists.", cause);
            default:
                throw cause;
        }
    }
}
