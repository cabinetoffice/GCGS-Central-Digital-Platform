using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;

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
        var pponIdentifier = @event.FindIdentifierByScheme(Events.Identifier.PponSchemeName);

        if (pponIdentifier != null)
        {
            var pponToUpdate = await pponRepository.FindPponByPponIdAsync(pponIdentifier.Id);

            if (pponToUpdate != null)
            {
                var identifiersToPersist = Persistence.Identifier.GetPersistenceIdentifiers(@event.AllIdentifiers());

                foreach (var identifier in identifiersToPersist)
                {
                    pponToUpdate.Identifiers.Add(identifier);
                }

                pponRepository.Save(pponToUpdate);
            }
        }
    }
}