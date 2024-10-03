using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

public interface IChoiceProviderStrategy
{
    public string AnswerFieldName { get; }
    Task<Dictionary<string, string>?> Execute(FormQuestionOptions options);
    public Task<string?> RenderOption(CO.CDP.OrganisationApp.Models.FormAnswer? answer);
    public Task<string?> RenderOption(CO.CDP.Forms.WebApiClient.FormAnswer? answer);
}