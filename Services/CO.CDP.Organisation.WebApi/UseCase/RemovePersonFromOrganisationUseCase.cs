using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonFromOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository) : IUseCase<(Guid OrganisationId, Guid PersonId), bool>
{
    public async Task<bool> Execute((Guid OrganisationId, Guid PersonId) command)
    {
        var organisation = await organisationRepository.Find(command.OrganisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.OrganisationId}.");

        // var person = await personRepository.Find(command.PersonId)
        //     ?? throw new RegisterOrganisationUseCase.RegisterOrganisationException.UnknownPersonException($"Unknown person {command.PersonId}.");

        var persons = await personRepository.FindByOrganisation(command.OrganisationId);

        var person = persons.FirstOrDefault(p => p.Guid == command.PersonId);

        if (person == null) return await Task.FromResult(false);


        // TODO: Ask how adding and removing relationships works - Also prevent removal of own person record
        // organisation.OrganisationPersons.Remove(organisationPerson);
        //
        // organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}