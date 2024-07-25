using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationUpdatedSubscriber(
    IPponRepository pponRepository,
    ILogger<OrganisationUpdatedSubscriber> logger)
    : ISubscriber<OrganisationUpdated>
{
    public async Task Handle(OrganisationUpdated @event)
    {
        // Find org based on Ppon id
        // Update identitfiers for organisation

        var pponToUpdate  = await pponRepository.FindPponByPponIdAsync(@event.PponId);

        if (pponToUpdate != null)
        {
            // Update identifiers
            pponRepository.UpdatePponIdentifiersAsync(pponToUpdate, @event.AllIdentifiers());
        }
        else
        {
            pponToUpdate = await pponRepository.FindPponByIdentifierAsync(@event.AllIdentifiers());

            if (pponToUpdate != null)
            {
                pponToUpdate.IdentifierId = @event.PponId;

                pponRepository.UpdatePponIdentifiersAsync(pponToUpdate, @event.AllIdentifiers());
            }
            else
            {
                logger.LogError("Organisation not found by Ppon Id or any supplied Identifier(s).");
            }
        }

        if (pponToUpdate != null)
        {
            pponRepository.Save(pponToUpdate);
        }
    }
}