using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp;

public interface IFormsEngine
{
    Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid formId, Guid sectionId);
    Task<FormQuestion?> GetNextQuestion(Guid formId, Guid sectionId, Guid currentQuestionId);
    Task<FormQuestion?> GetPreviousQuestion(Guid formId, Guid sectionId, Guid currentQuestionId);
    Task<FormQuestion?> GetCurrentQuestion(Guid formId, Guid sectionId, Guid questionId);
}
