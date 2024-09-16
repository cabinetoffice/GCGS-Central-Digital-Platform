using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IFormRepository : IDisposable
{
    Task<IEnumerable<FormSectionSummary>> GetFormSummaryAsync(Guid formId, Guid organisationId);
    Task SaveFormAsync(Form formSection);
    Task SaveSharedConsentAsync(SharedConsent sharedConsent);
    Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId);
    Task<SharedConsent?> GetSharedConsentWithAnswersAsync(Guid formId, Guid organisationId);

    Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId);

    Task<List<FormAnswerSet>> GetFormAnswerSetsFromCurrentSharedConsentAsync(Guid sectionId, Guid organisationId);
    Task<FormAnswerSet?> GetFormAnswerSetAsync(Guid organisationId, Guid answerSetId);
    Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId);
}