using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;

namespace CO.CDP.OrganisationApp;

public interface IAnswerDisplayService
{
    Task<string> FormatAnswerForDisplayAsync(QuestionAnswer questionAnswer, FormQuestion question);
    Task<AnswerSummary?> CreateIndividualAnswerSummaryAsync(FormQuestion question, FormQuestionAnswerState answerState, Guid organisationId, Guid formId, Guid sectionId);
    Task<GroupedAnswerSummary> CreateMultiQuestionGroupAsync(FormQuestion startingQuestion, List<FormQuestion> orderedJourney, FormQuestionAnswerState answerState, Guid organisationId, Guid formId, Guid sectionId, FormQuestionGrouping grouping, Func<List<FormQuestion>, FormQuestion?> getFirstQuestion);
}