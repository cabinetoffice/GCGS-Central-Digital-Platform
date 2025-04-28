using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public interface IEvaluator
{
    ExpressionOperationType OperationType { get; }
    string Evaluate(string expression, IEnumerable<object?> parameters);
}
