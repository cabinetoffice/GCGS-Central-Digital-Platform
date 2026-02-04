using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class BulkLookupPersonUseCase(IPersonRepository personRepository, IMapper mapper)
    : IUseCase<BulkLookupPerson, IReadOnlyDictionary<Guid, BulkLookupPersonResult>>
{
    public async Task<IReadOnlyDictionary<Guid, BulkLookupPersonResult>> Execute(BulkLookupPerson query)
    {
        if (query.Urns.Count == 0)
        {
            return new Dictionary<Guid, BulkLookupPersonResult>();
        }

        var persons = await Task.WhenAll(query.Urns.Select(personRepository.FindByUrn));
        return persons
            .Where(person => person != null)
            .Select(person => mapper.Map<BulkLookupPersonResult>(person))
            .ToDictionary(person => person.Id);
    }
}
