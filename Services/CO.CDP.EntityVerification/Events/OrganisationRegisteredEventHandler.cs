using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler(IPponService pponService, IPponRepository pponRepository)
    : IEventHandler<OrganisationRegistered>
{
    public Task Handle(OrganisationRegistered @event)
    {
        var pponid = pponService.GeneratePponId();

        Ppon newIdentifier = new() { PponId = pponid };

        pponRepository.Save(newIdentifier);
        return Task.CompletedTask;
    }
}