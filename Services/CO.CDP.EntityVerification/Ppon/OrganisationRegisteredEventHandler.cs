using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;
using Microsoft.AspNetCore.Components;
using System.Transactions;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationRegisteredEventHandler(IPponService pponService, IPponRepository pponRepository, IPublisher publisher)
    : IEventHandler<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        var pponId = pponService.GeneratePponId();

        Persistence.Ppon newIdentifier = new() { PponId = pponId };

        pponRepository.Save(newIdentifier);
        await publisher.Publish(newIdentifier);
    }
}