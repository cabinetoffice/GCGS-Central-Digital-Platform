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
        var pponIdentifier = @event.FindIdentifierByScheme(IdentifierSchemes.Ppon);

        if (pponIdentifier != null)
        {
            var pponToUpdate = await pponRepository.FindPponByPponIdAsync(pponIdentifier.Id);

            if (pponToUpdate != null)
            {
                // Add new identifiers that do not already exist.
                var newEventIdentifiers = @event.AllIdentifiers()
                    .Where(pi => !pponToUpdate.Identifiers.Any(i => i.IdentifierId == pi.Id && i.Scheme == pi.Scheme))
                    .Where(pi => pi.Scheme != IdentifierSchemes.Ppon)
                    .ToList();
                var identifiersToPersist = Persistence.Identifier.GetPersistenceIdentifiers(newEventIdentifiers);

                foreach (var identifier in identifiersToPersist)
                {
                    pponToUpdate.Identifiers.Add(identifier);
                }

                pponRepository.Save(pponToUpdate);
            }
        }
    }
}