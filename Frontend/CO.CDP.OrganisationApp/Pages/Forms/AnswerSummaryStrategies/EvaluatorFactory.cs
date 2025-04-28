using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class EvaluatorFactory
{
    private readonly Dictionary<ExpressionOperationType, IEvaluator> _evaluators;

    public EvaluatorFactory(IEnumerable<IEvaluator> evaluators)
    {
        _evaluators = evaluators.ToDictionary(e => e.OperationType);
    }

    public IEvaluator CreateEvaluator(ExpressionOperationType operationType)
    {
        if (!_evaluators.TryGetValue(operationType, out var evaluator))
            throw new NotSupportedException($"No evaluator registered for: {operationType}");

        return evaluator;
    }

    public static string GroupedSingleChoiceAnswerString(FormAnswer? answer, FormQuestion question)
    {
        if (answer?.OptionValue == null) return "";

        var choices = question.Options.Groups.SelectMany(g => g.Choices);

        var choiceOption = choices.FirstOrDefault(c => c.Value == answer.OptionValue);

        return (choiceOption == null ? answer.OptionValue : choiceOption.Title) ?? "";
    }
}
