using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;
using static CO.CDP.EntityVerification.Ppon.OrganisationUpdatedSubscriber.OrganisationUpdatedException;

namespace CO.CDP.EntityVerification.Ppon;

public class OrganisationUpdatedSubscriber(
    IPponRepository pponRepository)
    : ISubscriber<OrganisationUpdated>
{
    public class OrganisationUpdatedException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class NotFoundPponException(string message, Exception? cause = null)
            : Exception(message, cause);
    }

    public async Task Handle(OrganisationUpdated @event)
    {
        var pponToUpdate  = await pponRepository.FindPponByPponIdAsync(@event.PponId);

        if (pponToUpdate != null)
        {
            var identifiersToPersist = Persistence.Identifier.GetPersistenceIdentifiers(@event.AllIdentifiers());

            foreach (var identifier in identifiersToPersist)
            {
                pponToUpdate.Identifiers.Add(identifier);
            }

            pponRepository.Save(pponToUpdate);
        }
        else
        {
            throw new NotFoundPponException($"Not found Ppon for id: {@event.PponId}");
        }
    }
}