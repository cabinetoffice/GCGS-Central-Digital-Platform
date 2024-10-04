namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
public class ChoiceProviderService(IServiceProvider serviceProvider) : IChoiceProviderService
{
    public IChoiceProviderStrategy GetStrategy(string? strategyType)
    {
        strategyType ??= "DefaultChoiceProviderStrategy";
        return serviceProvider.GetKeyedService<IChoiceProviderStrategy>(strategyType)!;
    }
}
