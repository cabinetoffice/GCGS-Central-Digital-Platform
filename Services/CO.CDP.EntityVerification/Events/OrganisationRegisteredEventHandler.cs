using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler(IPponService pponService, IPponRepository pponRepository) : IEventHandler
{
    public void Action(IEvent msg)
    {
        OrganisationRegistered newOrg = (OrganisationRegistered)msg;
        var pponid = pponService.GeneratePponId();

        Ppon newIdentifier = new() { PponId = pponid };

        pponRepository.Save(newIdentifier);
    }
}