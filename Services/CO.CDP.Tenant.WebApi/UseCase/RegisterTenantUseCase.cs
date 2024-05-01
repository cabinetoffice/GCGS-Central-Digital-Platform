using AutoMapper;
using CO.CDP.Persistence.OrganisationInformation;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class RegisterTenantUseCase(ITenantRepository tenantRepository, IMapper mapper, Func<Guid> guidFactory)
    : IUseCase<RegisterTenant, Model.Tenant>
{
    public RegisterTenantUseCase(ITenantRepository tenantRepository, IMapper mapper)
        : this(tenantRepository, mapper, Guid.NewGuid)
    {
    }

    public Task<Model.Tenant> Execute(RegisterTenant command)
    {
        var tenant = mapper.Map<Persistence.OrganisationInformation.Tenant>(command, o => o.Items["Guid"] = guidFactory());
        tenantRepository.Save(tenant);
        return Task.FromResult(mapper.Map<Model.Tenant>(tenant));
    }
}