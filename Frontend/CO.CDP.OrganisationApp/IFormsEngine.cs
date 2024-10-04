using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp;

public interface IFormsEngine
{
    Task<SectionQuestionsResponse> GetFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId);

    Task<FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId);

    Task<FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId);

    Task<FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId);

    Task SaveUpdateAnswers(Guid formId, Guid sectionId, Guid organisationId, FormQuestionAnswerState answerSet);
    Task<string> CreateShareCodeAsync(Guid formId, Guid organisationId);
}