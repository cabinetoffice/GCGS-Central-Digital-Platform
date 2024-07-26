using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseFormRepository(OrganisationInformationContext context) : IFormRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId)
    {
        return await context.Set<FormSection>()
            .Include(s => s.Questions)

            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Guid == sectionId);
    }

    public async Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId)
    {
        return await context.Set<FormQuestion>().Where(q => q.Section.Guid == sectionId).ToListAsync();
    }

    public async Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId)
    {
        var anserSet = await context.Set<FormAnswerSet>().FirstOrDefaultAsync(a => a.Organisation.Guid == organisationId && a.Guid == answerSetId);
        if (anserSet == null) return false;

        anserSet.Deleted = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task SaveFormAsync(Form form)
    {
        context.Update(form);
        await context.SaveChangesAsync();
    }

    public async Task<List<FormAnswerSet>> GetFormAnswerSetsAsync(Guid sectionId, Guid organisationId)
    {
        return await context.Set<FormAnswerSet>()
            .Include(a => a.Answers)
            .Where(a => a.Section.Guid == sectionId && a.Organisation.Guid == organisationId)
            .ToListAsync();
    }

    public async Task<FormSection?> GetFormSectionAsync(Guid sectionId)
    {
        return await context.Set<FormSection>()
            .FirstOrDefaultAsync(s => s.Guid == sectionId);
    }

    public Task<bool> Save(Guid sectionId, Guid answerSetId, IEnumerable<FormAnswer> updatedAnswers)
    {
        throw new NotImplementedException();
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