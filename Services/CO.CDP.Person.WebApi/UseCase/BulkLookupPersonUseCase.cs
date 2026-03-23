using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class BulkLookupPersonUseCase(IPersonRepository personRepository, IMapper mapper, ILogger<BulkLookupPersonUseCase> logger)
    : IUseCase<BulkLookupPerson, IReadOnlyDictionary<Guid, BulkLookupPersonResult>>
{
    public async Task<IReadOnlyDictionary<Guid, BulkLookupPersonResult>> Execute(BulkLookupPerson query)
    {
        if (query.Urns.Count == 0)
        {
            return new Dictionary<Guid, BulkLookupPersonResult>();
        }

        var parsedIds = query.Urns
            .Select(urn => Guid.TryParse(urn, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        logger.LogDebug("Bulk lookup received {GuidCount} GUIDs", parsedIds.Count);

        var personsByGuidTask = Task.WhenAll(parsedIds.Select(personRepository.Find));
        var persons = (await Task.WhenAll(personsByGuidTask))
            .SelectMany(result => result)
            .Where(person => person != null)
            .Select(person => mapper.Map<BulkLookupPersonResult>(person))
            .GroupBy(person => person.Id)
            .Select(group => group.First())
            .ToDictionary(person => person.Id);

        logger.LogDebug("Bulk lookup returning {PersonCount} persons", persons.Count);
        return persons;
    }
}
