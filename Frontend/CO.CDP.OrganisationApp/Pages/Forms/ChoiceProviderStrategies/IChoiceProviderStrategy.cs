using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

public interface IChoiceProviderStrategy
{
    Task<List<string>?> Execute(FormQuestionOptions options);
}