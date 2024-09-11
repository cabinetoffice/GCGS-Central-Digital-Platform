using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonFromOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository) : IUseCase<(Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var persons = await personRepository.FindByOrganisation(command.organisationId);

        var person = persons.FirstOrDefault(p => p.Guid == command.removePersonFromOrganisation.PersonId);

        if (person == null) return await Task.FromResult(false);

        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.PersonId == person.Id);

        if (organisationPerson == null) return await Task.FromResult(false);

        organisation.OrganisationPersons.Remove(organisationPerson);

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}