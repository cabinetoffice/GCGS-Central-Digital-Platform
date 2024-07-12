using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler : IEventHandler
{
    private readonly IPponService _pponService;
    private readonly IServiceProvider _serviceProvider;

    public OrganisationRegisteredEventHandler(IPponService pponService, IServiceProvider serviceProvider)
    {
        _pponService = pponService;
        _serviceProvider = serviceProvider;
    }

    public void Action(EvMessage msg)
    {
        OrganisationRegisteredMessage newOrg = (OrganisationRegisteredMessage)msg;
        var pponid = _pponService.GeneratePponId(newOrg.Scheme, newOrg.GovIdentifier);

        Ppon newIdentifier = new Ppon() { PponId  = pponid };

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EntityValidationContext>();
            DatabasePponRepository db = new DatabasePponRepository(dbContext);

            db.Save(newIdentifier);
        }
    }
}
