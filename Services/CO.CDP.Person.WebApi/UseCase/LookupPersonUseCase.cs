using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.Person.WebApi.UseCase;
public class LookupPersonUseCase(IPersonRepository personRepository, IMapper mapper) : IUseCase<LookupPerson, Model.Person?>
{
    public async Task<Model.Person?> Execute(LookupPerson query)
    {
        if(!string.IsNullOrEmpty(query.Urn)) {
            return await personRepository.FindByUrn(query.Urn)
                .AndThen(mapper.Map<Model.Person>);
        }

        if (!string.IsNullOrEmpty(query.Email))
        {
            return await personRepository.FindByEmail(query.Email)
                .AndThen(mapper.Map<Model.Person>);
        }

        return null;
    }
}