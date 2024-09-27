using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
public class ChoiceProviderService(IEnumerable<IChoiceProviderStrategy> choiceProviderStrategies, IServiceProvider serviceProvider) : IChoiceProviderService
{
    public IChoiceProviderStrategy GetStrategy(string strategyType)
    {
        strategyType ??= "DefaultChoiceProviderStrategy";
        return serviceProvider.GetKeyedService<IChoiceProviderStrategy>(strategyType)!;
    }
}
