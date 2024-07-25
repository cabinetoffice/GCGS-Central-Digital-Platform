using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IFormRepository : IDisposable
{
    Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId);
    Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId);
    Task<bool> DeleteAnswerSetAsync(Guid organisationId, Guid answerSetId);
    Task SaveFormAsync(Form formSection);
}