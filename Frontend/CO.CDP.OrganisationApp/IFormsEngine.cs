using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp;

public interface IFormsEngine
{
    Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId);

    Task<FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId);

    Task<FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId);

    Task<FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId);
}