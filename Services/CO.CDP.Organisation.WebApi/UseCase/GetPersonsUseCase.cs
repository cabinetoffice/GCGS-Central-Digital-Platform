using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetPersonsUseCase(IPersonRepository personRepository, IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.Person>>
{
    public async Task<IEnumerable<Model.Person>> Execute(Guid organisationId)
    {
        var organisation = await organisationRepository.Find(organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var persons = await personRepository.FindByOrganisation(organisationId);

        var personModels = new List<Model.Person>();

        foreach (var person in persons)
        {
            var personModel = mapper.Map<Model.Person>(person);
            var personOrganisation = person.PersonOrganisations.FirstOrDefault(po => po.OrganisationId == organisation.Id);

            if (personOrganisation == null) continue;
            personModel.Scopes = personOrganisation.Scopes;
            personModels.Add(personModel);
        }

        return personModels;
    }
}