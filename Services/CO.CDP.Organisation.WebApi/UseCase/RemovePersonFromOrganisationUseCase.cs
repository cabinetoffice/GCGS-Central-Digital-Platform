using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonFromOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPublisher publisher,
    ILogger<RemovePersonFromOrganisationUseCase> logger)
    : IUseCase<(Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation), bool>
{
    public async Task<bool> Execute(
        (Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation) command)
    {
        var organisation = await organisationRepository.FindIncludingPersons(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var personId = command.removePersonFromOrganisation.PersonId;
        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.Person.Guid == personId);
        var personWithTenant = organisation.Tenant.Persons.FindLast(tp => tp.Guid == personId);

        if (organisationPerson == null && personWithTenant == null) return false;

        if (personWithTenant != null)
            organisation.Tenant.Persons.Remove(personWithTenant);

        if (organisationPerson != null)
            organisation.OrganisationPersons.Remove(organisationPerson);

        logger.LogInformation("Publishing PersonRemovedFromOrganisation for org {OrgId}, person {PersonId}",
            command.organisationId, personId);

        await organisationRepository.SaveAsync(organisation,
            async _ => await publisher.Publish(new PersonRemovedFromOrganisation
            {
                OrganisationId = command.organisationId.ToString(),
                PersonId = personId.ToString()
            }));

        return true;
    }
}