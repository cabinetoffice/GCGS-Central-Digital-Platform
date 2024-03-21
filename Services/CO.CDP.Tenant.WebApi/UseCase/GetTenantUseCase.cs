using AutoMapper;
using CO.CDP.Tenant.Persistence;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class GetTenantUseCase(ITenantRepository tenantRepository, IMapper mapper) : IUseCase<Guid, Model.Tenant?>
{
    public async Task<Model.Tenant?> Execute(Guid tenantId)
    {
        return await tenantRepository.Find(tenantId)
            .AndThen(mapper.Map<Model.Tenant>);
    }
}