using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationRegisteredEventHandler(IPponService pponService, IPponRepository pponRepository)
    : IEventHandler<OrganisationRegistered>
{
    public Task Handle(OrganisationRegistered @event)
    {
        var pponId = pponService.GeneratePponId();

        Persistence.Ppon newIdentifier = new() { PponId = pponId };

        pponRepository.Save(newIdentifier);
        return Task.CompletedTask;
    }
}