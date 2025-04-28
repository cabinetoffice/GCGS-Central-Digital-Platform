using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class StringFormatEvaluator : IEvaluator
{
    public ExpressionOperationType OperationType => ExpressionOperationType.StringFormat;
    public string Evaluate(string expression, IEnumerable<object?> parameters)
    {
        return string.Format(expression, parameters.ToArray());
    }
}
