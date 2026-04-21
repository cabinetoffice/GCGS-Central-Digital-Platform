using CO.CDP.MQ;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Events;

public class PersonRemovedFromOrganisationHandler(
    IOrganisationRepository organisationRepository,
    ILogger<PersonRemovedFromOrganisationHandler> logger
) : ISubscriber<PersonRemovedFromOrganisation>
{
    public async Task Handle(PersonRemovedFromOrganisation @event)
    {
        if (!Guid.TryParse(@event.OrganisationId, out var organisationId) ||
            !Guid.TryParse(@event.PersonId, out var personId))
        {
            logger.LogWarning(
                "PersonRemovedFromOrganisation: invalid Guid(s) - OrganisationId={OrganisationId}, PersonId={PersonId}",
                @event.OrganisationId, @event.PersonId);
            return;
        }

        var organisation = await organisationRepository.FindIncludingPersons(organisationId);
        if (organisation is null)
        {
            logger.LogWarning(
                "PersonRemovedFromOrganisation: organisation {OrganisationId} not found, skipping.",
                organisationId);
            return;
        }

        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.Person.Guid == personId);
        var personWithTenant = organisation.Tenant.Persons.FindLast(tp => tp.Guid == personId);

        if (organisationPerson is null && personWithTenant is null)
        {
            logger.LogInformation(
                "PersonRemovedFromOrganisation: person {PersonId} not found in organisation {OrganisationId}, already removed.",
                personId, organisationId);
            return;
        }

        if (personWithTenant is not null)
            organisation.Tenant.Persons.Remove(personWithTenant);

        if (organisationPerson is not null)
            organisation.OrganisationPersons.Remove(organisationPerson);

        await organisationRepository.SaveAsync(organisation, _ => Task.CompletedTask);

        logger.LogInformation(
            "PersonRemovedFromOrganisation: person {PersonId} removed from organisation {OrganisationId}.",
            personId, organisationId);
    }
}