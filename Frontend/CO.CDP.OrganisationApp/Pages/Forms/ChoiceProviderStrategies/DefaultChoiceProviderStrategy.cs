namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using CO.CDP.Forms.WebApiClient;

public class DefaultChoiceProviderStrategy() : IChoiceProviderStrategy
{
    public async Task<List<string>?> Execute(FormQuestionOptions options)
    {
        return await Task.FromResult(options.Choices.Select(c => c.Title).ToList());
    }
}