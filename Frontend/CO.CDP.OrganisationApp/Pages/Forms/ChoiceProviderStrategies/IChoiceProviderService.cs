namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

public interface IChoiceProviderService
{
    IChoiceProviderStrategy GetStrategy(string? strategyType);
}