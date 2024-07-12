using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler : IEventHandler
{
    private readonly IPponService _pponService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceProviderWrapper _spWrapper;

    public OrganisationRegisteredEventHandler(IPponService pponService,
        IServiceProvider serviceProvider,
        IServiceProviderWrapper spWrapper)
    {
        _pponService = pponService;
        _serviceProvider = serviceProvider;
        _spWrapper = spWrapper;
    }

    public void Action(EvMessage msg)
    {
        OrganisationRegisteredMessage newOrg = (OrganisationRegisteredMessage)msg;
        var pponid = _pponService.GeneratePponId(newOrg.Scheme, newOrg.GovIdentifier);

        Ppon newIdentifier = new() { PponId  = pponid };

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = _spWrapper.GetRequiredService(scope.ServiceProvider);
            DatabasePponRepository db = new DatabasePponRepository(dbContext);

            db.Save(newIdentifier);
        }
    }
}
