using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class RegisterTenantUseCase(ITenantRepository tenantRepository, Func<Guid> guidFactory)
    : IUseCase<RegisterTenant, Model.Tenant>
{
    public RegisterTenantUseCase(ITenantRepository tenantRepository) : this(tenantRepository, Guid.NewGuid)
    {
    }

    public Task<Model.Tenant> Execute(RegisterTenant command)
    {
        var tenant = new Persistence.Tenant
        {
            Guid = guidFactory(),
            Name = command.Name,
            ContactInfo = new Persistence.Tenant.TenantContactInfo
            {
                Email = command.ContactInfo.Email,
                Phone = command.ContactInfo.Phone
            }
        };
        tenantRepository.Save(tenant);
        return Task.FromResult(new Model.Tenant
        {
            Id = tenant.Guid,
            Name = tenant.Name,
            ContactInfo = new TenantContactInfo
            {
                Email = tenant.ContactInfo.Email,
                Phone = tenant.ContactInfo.Phone
            }
        });
    }
}