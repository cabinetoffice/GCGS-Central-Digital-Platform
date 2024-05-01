using AutoMapper;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.OrganisationInformation.Tenant, Model.Tenant>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<TenantContactInfo, Persistence.OrganisationInformation.Tenant.TenantContactInfo>();
        CreateMap<Persistence.OrganisationInformation.Tenant.TenantContactInfo, TenantContactInfo>();

        CreateMap<RegisterTenant, Persistence.OrganisationInformation.Tenant>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore());
    }
}