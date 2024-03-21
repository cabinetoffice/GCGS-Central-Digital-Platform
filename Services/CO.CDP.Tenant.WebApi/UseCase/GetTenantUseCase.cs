using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class GetTenantUseCase(ITenantRepository tenantRepository) : IUseCase<Guid, Model.Tenant?>
{
    public async Task<Model.Tenant?> Execute(Guid tenantId)
    {
        return await tenantRepository.Find(tenantId)
            .AndThen(tenant => tenant != null
                ? new Model.Tenant
                {
                    Id = tenant.Guid,
                    Name = tenant.Name,
                    ContactInfo = new TenantContactInfo
                    {
                        Email = tenant.ContactInfo.Email,
                        Phone = tenant.ContactInfo.Phone
                    }
                }
                : null);
    }
}