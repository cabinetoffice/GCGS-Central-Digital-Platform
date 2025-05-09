using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public interface IAnswerSummaryStrategy
{
    Task<List<AnswerSummary>> GetAnswerSummaries(SectionQuestionsResponse form, FormAnswerSet answerSet);
}