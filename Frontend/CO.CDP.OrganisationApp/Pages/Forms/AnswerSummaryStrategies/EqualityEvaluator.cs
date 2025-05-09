using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class EqualityEvaluator : IEvaluator
{
    public ExpressionOperationType OperationType => ExpressionOperationType.Equality;
    public string Evaluate(string expression, IEnumerable<object?> parameters)
    {
        var formatted = string.Format(expression, parameters.ToArray());

        var parts = formatted.Split(new[] { '?', ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new FormatException("Invalid ternary expression format.");

        var condition = parts[0].Trim();
        var trueResult = parts[1].Trim().Trim('"');
        var falseResult = parts[2].Trim().Trim('"');

        var conditionParts = condition.Split(new[] { "==" }, StringSplitOptions.None);
        if (conditionParts.Length != 2)
            throw new FormatException("Invalid equality condition.");

        var left = conditionParts[0].Trim().Trim('"');
        var right = conditionParts[1].Trim().Trim('"');

        return left == right ? trueResult : falseResult;
    }
}
