using CO.CDP.MQ;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Events;

public class PersonScopesUpdatedHandler(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository,
    ILogger<PersonScopesUpdatedHandler> logger
) : ISubscriber<PersonScopesUpdated>
{
    public async Task Handle(PersonScopesUpdated @event)
    {
        if (!Guid.TryParse(@event.OrganisationId, out var organisationId) ||
            !Guid.TryParse(@event.PersonId, out var personId))
        {
            logger.LogWarning(
                "PersonScopesUpdated: invalid Guid(s) - OrganisationId={OrganisationId}, PersonId={PersonId}",
                @event.OrganisationId, @event.PersonId);
            return;
        }

        var organisation = await organisationRepository.FindIncludingPersons(organisationId);
        if (organisation is null)
        {
            logger.LogWarning(
                "PersonScopesUpdated: organisation {OrganisationId} not found, skipping.",
                organisationId);
            return;
        }

        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.Person.Guid == personId);
        if (organisationPerson is null)
        {
            var person = await personRepository.Find(personId);
            if (person is null)
            {
                logger.LogWarning(
                    "PersonScopesUpdated: person {PersonId} not found, skipping.",
                    personId);
                return;
            }

            organisationPerson = new OrganisationPerson
            {
                Person = person,
                Organisation = organisation,
                Scopes = []
            };
            organisation.OrganisationPersons.Add(organisationPerson);

            if (!organisation.Tenant.Persons.Exists(tp => tp.Guid == personId))
                organisation.Tenant.Persons.Add(person);
        }

        organisationPerson.Scopes = @event.Scopes;

        await organisationRepository.SaveAsync(organisation, _ => Task.CompletedTask);

        logger.LogInformation(
            "PersonScopesUpdated: scopes updated for person {PersonId} in organisation {OrganisationId}.",
            personId, organisationId);
    }
}