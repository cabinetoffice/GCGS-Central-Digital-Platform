using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IFormRepository : IDisposable
{
    #region Form Methods
    Task SaveFormAsync(Form formSection);
    Task SaveSharedConsentAsync(SharedConsent sharedConsent);
    Task<FormSection?> GetFormSectionAsync(Guid sectionId);
    Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId);
    #endregion

    #region Shared Consents Methods
    Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId);
    #endregion

    #region Question Methods
    Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId);
    #endregion

    #region Answer Set Methods
    Task<List<FormAnswerSet>> GetFormAnswerSetsAsync(Guid sectionId, Guid organisationId);
    Task<FormAnswerSet?> GetFormAnswerSetAsync(Guid sectionId, Guid organisationId, Guid answerSetId);
    Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId);
    Task SaveAnswerSet(FormAnswerSet answerSet);
    #endregion

    public class FormRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateFormAnswerSetException(string message, Exception? cause = null)
            : FormRepositoryException(message, cause);
    }
}