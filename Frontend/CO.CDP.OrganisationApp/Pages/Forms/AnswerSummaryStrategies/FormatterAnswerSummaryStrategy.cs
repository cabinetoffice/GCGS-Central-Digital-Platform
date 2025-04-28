using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class FormatterAnswerSummaryStrategy : IAnswerSummaryStrategy
{
    private readonly EvaluatorFactory _evaluatorFactory;
    private readonly IChoiceProviderService _choiceProviderService;

    public FormatterAnswerSummaryStrategy(
        EvaluatorFactory evaluatorFactory
        , IChoiceProviderService choiceProviderService)
    {
        _evaluatorFactory = evaluatorFactory;
        _choiceProviderService = choiceProviderService;
    }

    public async Task<List<AnswerSummary>> GetAnswerSummaries(SectionQuestionsResponse form, FormAnswerSet answerSet)
    {
        var summaryFormatter = form.Section.Configuration.SummaryRenderFormatter;
        var keyParamsResolved = await ResolveParamsAsync(summaryFormatter.KeyParams, form, answerSet);
        var valueParamsResolved = await ResolveParamsAsync(summaryFormatter.ValueParams, form, answerSet);

        var keyEvaluator = _evaluatorFactory.CreateEvaluator(summaryFormatter.KeyExpressionOperation);
        var valueEvaluator = _evaluatorFactory.CreateEvaluator(summaryFormatter.ValueExpressionOperation);

        return new List<AnswerSummary>
            {
                new AnswerSummary
                {
                    Title = keyEvaluator.Evaluate(summaryFormatter.KeyExpression, keyParamsResolved),
                    Answer = valueEvaluator.Evaluate(summaryFormatter.ValueExpression, valueParamsResolved),
                }
            };
    }

    private async Task<object[]> ResolveParamsAsync(IEnumerable<string> paramTitles, SectionQuestionsResponse form, FormAnswerSet answerSet)
    {
        var tasks = paramTitles.Select(async name =>
        {
            var question = form.Questions.First(q => q.Name == name);
            var answer = answerSet.Answers.First(a => a.QuestionId == question.Id);
            return await GetAnswerObject(answer, question);
        });

        return (await Task.WhenAll(tasks))!;
    }

    private async Task<object?> GetAnswerObject(FormAnswer answer, FormQuestion question)
    {
        async Task<string> singleChoiceString(FormAnswer a)
        {
            var choiceProviderStrategy = _choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
            return await choiceProviderStrategy.RenderOption(a) ?? "";
        }

        return question.Type switch
        {
            FormQuestionType.YesOrNo => answer.BoolValue,
            FormQuestionType.Text => answer.TextValue,
            FormQuestionType.FileUpload => answer.TextValue,
            FormQuestionType.SingleChoice => await singleChoiceString(answer),
            FormQuestionType.Date => answer.DateValue,
            FormQuestionType.CheckBox => answer.BoolValue.HasValue ? question.Options.Choices?.FirstOrDefault()?.Title : null,
            FormQuestionType.Address => answer.AddressValue != null ? $"{answer.AddressValue.StreetAddress}, {answer.AddressValue.Locality}, {answer.AddressValue.PostalCode}, {answer.AddressValue.CountryName}" : null,
            FormQuestionType.MultiLine => answer.TextValue,
            FormQuestionType.GroupedSingleChoice => EvaluatorFactory.GroupedSingleChoiceAnswerString(answer, question),
            FormQuestionType.Url => answer.TextValue,
            _ => ""
        };
    }
}