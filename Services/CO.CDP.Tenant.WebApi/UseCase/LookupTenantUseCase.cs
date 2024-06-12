using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class LookupTenantUseCase(IPersonRepository personRepository, IMapper mapper)
    : IUseCase<string, Model.TenantLookup?>
{
    public async Task<Model.TenantLookup?> Execute(string urn)
    {
        return await personRepository.FindByUserUrn(urn)
            .AndThen(mapper.Map<Model.TenantLookup>);
    }
}