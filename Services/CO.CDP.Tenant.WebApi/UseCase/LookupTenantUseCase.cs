using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class LookupTenantUseCase(ITenantRepository tenantRepository, IMapper mapper)
    : IUseCase<string, Model.TenantLookup?>
{
    public async Task<Model.TenantLookup?> Execute(string userUrn)
    {
        return await tenantRepository.LookupTenant(userUrn)
            .AndThen(mapper.Map<Model.TenantLookup>);
    }
}