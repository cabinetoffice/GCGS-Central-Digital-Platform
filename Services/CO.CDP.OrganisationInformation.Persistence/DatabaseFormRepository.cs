using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseFormRepository(OrganisationInformationContext context) : IFormRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    #region Form Methods

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

    #endregion

    #region Shared Consents Methods

    public async Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Draft)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Organisation.Guid == organisationId);
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
            .Where(x => x.SubmissionState == SubmissionState.Submitted && x.Organisation.Guid == organisationId).ToListAsync();            
    }

    #endregion

    #region Question Methods

    public async Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId)
    {
        return await context.Set<FormQuestion>().Where(q => q.Section.Guid == sectionId).ToListAsync();
    }

    #endregion

    #region Answer Set Methods

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

    #endregion

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