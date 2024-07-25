using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IFormRepository : IDisposable
{
    Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId);
    Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId);
    Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId);
    Task SaveFormAsync(Form formSection);
    Task<FormSection?> GetFormSectionAsync(Guid sectionId);
    Task<List<FormAnswerSet>> GetFormAnswerSetsAsync(Guid sectionId, Guid organisationId);
    Task<bool> Save(Guid sectionId, Guid answerSetId, IEnumerable<FormAnswer> updatedAnswers);

    public class FormRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateFormAnswerSetException(string message, Exception? cause = null)
            : FormRepositoryException(message, cause);
    }
}