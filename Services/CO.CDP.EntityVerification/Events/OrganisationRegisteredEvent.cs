using Amazon.SQS.Model;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEvent : IEvEvent
{
    private readonly IPponService _pponService;

    public OrganisationRegisteredEvent(IPponService pponService)
    {
        _pponService = pponService;
    }

    public void Action(EvMessage msg)
    {
        OrganisationRegisteredMessage newOrg = (OrganisationRegisteredMessage)msg;
        var pponid = _pponService.GeneratePponId(newOrg.Scheme, newOrg.GovIdentifier);

        // TODO: store pponid in DB
    }
}
