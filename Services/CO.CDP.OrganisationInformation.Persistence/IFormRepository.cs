using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IFormRepository : IDisposable
{
    Task<IEnumerable<FormSectionSummary>> GetFormSummaryAsync(Guid formId, Guid organisationId);
    Task SaveSharedConsentAsync(SharedConsent sharedConsent);
    Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId);
    Task<SharedConsent?> GetSharedConsentWithAnswersAsync(Guid formId, Guid organisationId);

    Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId);

    Task<List<FormAnswerSet>> GetFormAnswerSetsFromCurrentSharedConsentAsync(Guid sectionId, Guid organisationId);
    Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId);
}