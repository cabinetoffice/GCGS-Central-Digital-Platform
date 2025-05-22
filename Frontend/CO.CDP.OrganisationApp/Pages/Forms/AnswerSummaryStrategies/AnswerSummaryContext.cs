using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class AnswerSummaryContext
{
    private readonly IAnswerSummaryStrategy _strategy;

    public AnswerSummaryContext(IAnswerSummaryStrategy strategy)
    {
        _strategy = strategy;
    }

    public Task<List<AnswerSummary>> GetAnswerSummaries(SectionQuestionsResponse form, FormAnswerSet answerSet)
    {
        return _strategy.GetAnswerSummaries(form, answerSet);
    }
}