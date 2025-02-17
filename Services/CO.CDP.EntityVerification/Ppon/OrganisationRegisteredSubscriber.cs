using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;
using static CO.CDP.EntityVerification.Events.Identifier;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationRegisteredSubscriber(
    IPponService pponService,
    IPponRepository pponRepository,
    IPublisher publisher)
    : ISubscriber<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        Persistence.Ppon newPpon = new()
        {
            IdentifierId = pponService.GeneratePponId(@event.Type),
            Name = @event.Name,
            OrganisationId = @event.Id,
            Identifiers = GetPersistenceIdentifiers(@event.AllIdentifiers())
        };

        await pponRepository.SaveAsync(newPpon, async _ => await publisher.Publish(new PponGenerated
        {
            OrganisationId = @event.Id,
            Id = newPpon.IdentifierId,
            LegalName = @event.Identifier.LegalName,
            Scheme = "GB-PPON"
        }));
    }
}