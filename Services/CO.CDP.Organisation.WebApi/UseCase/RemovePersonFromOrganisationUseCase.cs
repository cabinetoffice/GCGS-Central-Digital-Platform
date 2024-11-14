using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonFromOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository) : IUseCase<(Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation) command)
    {
        var organisation = await organisationRepository.FindIncludingPersons(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.Person.Guid == command.removePersonFromOrganisation.PersonId);
        var personWithTenant = organisation.Tenant.Persons.FindLast(tp => tp.Guid == command.removePersonFromOrganisation.PersonId);

        if (organisationPerson == null && personWithTenant == null) return false;

        if (personWithTenant != null)
        {
            organisation.Tenant.Persons.Remove(personWithTenant);
        }

        if (organisationPerson != null)
        {
            organisation.OrganisationPersons.Remove(organisationPerson);
        }

        organisationRepository.Save(organisation);

        return true;
    }
}