using AutoMapper;
using CO.CDP.Common;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class LookupTenantUseCase(ITenantRepository tenantRepository, IMapper mapper) : IUseCase<string, Model.Tenant?>
{
    public async Task<Model.Tenant?> Execute(string name)
    {
        return await tenantRepository.FindByName(name)
            .AndThen(mapper.Map<Model.Tenant>);
    }
}