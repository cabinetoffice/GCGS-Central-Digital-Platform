using AutoMapper;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.Tenant, Model.Tenant>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<TenantContactInfo, Persistence.Tenant.TenantContactInfo>();
        CreateMap<Persistence.Tenant.TenantContactInfo, TenantContactInfo>();

        CreateMap<RegisterTenant, Persistence.Tenant>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore());
    }
}