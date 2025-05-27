using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetPersonsInRoleUseCase(IPersonRepository personRepository, IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, string role), IEnumerable<Model.Person>>
{
    public async Task<IEnumerable<Model.Person>> Execute((Guid organisationId, string role) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var persons = await personRepository.FindByOrganisation(command.organisationId);

        var personModels = new List<Model.Person>();

        foreach (var person in persons)
        {
            var personModel = mapper.Map<Model.Person>(person);
            var personOrganisation = person.PersonOrganisations.FirstOrDefault(po => po.OrganisationId == organisation.Id);

            if (personOrganisation == null) continue;

            personModel.Scopes = personOrganisation.Scopes;

            if (personModel.Scopes.Select(item => item.ToLower()).Contains(command.role.ToLower()))
            {
                personModels.Add(personModel);
            }
        }

        return personModels;
    }
}
