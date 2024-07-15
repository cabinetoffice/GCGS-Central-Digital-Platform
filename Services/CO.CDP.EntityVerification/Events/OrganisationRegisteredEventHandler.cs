using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler(
    IPponService pponService,
    IPponRepository pponRepository,
    IServiceProvider services)
    : IEventHandler
{
    public void Action(EvMessage msg)
    {
        OrganisationRegisteredMessage newOrg = (OrganisationRegisteredMessage)msg;
        var pponid = pponService.GeneratePponId();

        Ppon newIdentifier = new() { PponId = pponid };

        using var scope = services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<EntityVerificationContext>();
        pponRepository.Save(dbContext, newIdentifier);
    }
}