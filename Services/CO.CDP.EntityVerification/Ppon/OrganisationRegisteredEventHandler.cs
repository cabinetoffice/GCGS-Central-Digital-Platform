using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;
using Identifier = CO.CDP.EntityVerification.Persistence.Identifier;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationRegisteredEventHandler(IPponService pponService,
    IPponRepository pponRepository)
    : IEventHandler<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        Persistence.Ppon newPpon = new() {
            PponId = pponService.GeneratePponId(),
            Name = @event.Name,
            OrganisationId = @event.Id
        };

        List<Identifier> ids = [];
        foreach (var e in @event.AllIdentifiers())
        {
            ids.Add(new Identifier { Ppon = newPpon,
                LegalName = e.LegalName,
                Scheme = e.Scheme,
                Uri = e.Uri });
        }

        newPpon.Identifiers = ids;

        pponRepository.Save(newPpon);
    }
}