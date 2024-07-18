using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationRegisteredEventHandler(IPponService pponService, IPponRepository pponRepository, IPublisher publisher)
    : IEventHandler<OrganisationRegistered>
{
    public Task Handle(OrganisationRegistered @event)
    {
        var pponId = pponService.GeneratePponId();

        Identifier newIdentifier = new () { Name  = "", Scheme = "" };
        Persistence.Ppon newPpon = new() { PponId = pponId, Name = @event.Name, Identifier = newIdentifier };

        pponRepository.Save(newPpon);
        publisher.Publish(newPpon);

        return Task.CompletedTask;
    }
}