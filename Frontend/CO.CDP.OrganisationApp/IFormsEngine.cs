using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;

namespace CO.CDP.OrganisationApp;

public interface IFormsEngine
{
    Task<SectionQuestionsResponse> GetFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId);

    Task<FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId, FormQuestionAnswerState? answerState);

    Task<FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId, FormQuestionAnswerState? answerState);

    Task<FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId);

    Task SaveUpdateAnswers(Guid formId, Guid sectionId, Guid organisationId, FormQuestionAnswerState answerSet);

    Task<string> CreateShareCodeAsync(Guid formId, Guid organisationId);

    Guid? GetPreviousUnansweredQuestionId(List<FormQuestion> questions, Guid currentQuestionId, FormQuestionAnswerState answerState);

    Task<MultiQuestionPageModel> GetMultiQuestionPage(Guid organisationId, Guid formId, Guid sectionId, Guid startingQuestionId);

    Task<List<IAnswerDisplayItem>> GetGroupedAnswerSummaries(Guid organisationId, Guid formId, Guid sectionId, FormQuestionAnswerState answerState);

    Task<FormQuestion?> GetFirstQuestion(Guid organisationId, Guid formId, Guid sectionId);
}