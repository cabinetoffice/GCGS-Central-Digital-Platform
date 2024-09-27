namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
public class ChoiceProviderService(IEnumerable<IChoiceProviderStrategy> choiceProviderStrategies) : IChoiceProviderService
{
    public IChoiceProviderStrategy GetStrategy(string strategyType)
    {
        IChoiceProviderStrategy? strategy = choiceProviderStrategies.FirstOrDefault(s => s.GetType().Name == strategyType);

        if (strategy == null)
        {
            strategy = choiceProviderStrategies.First(s => s.GetType().Name == "DefaultChoiceProviderStrategy");
        }

        return strategy;

        /*        return strategyType switch
                {
                    "ExclusionAppliesTo" => _serviceProvider.GetRequiredService<ExclusionAppliesToChoiceProviderStrategy>(),
                    _ => _serviceProvider.GetRequiredService<DefaultChoiceProviderStrategy>()
                };*/
    }
}
