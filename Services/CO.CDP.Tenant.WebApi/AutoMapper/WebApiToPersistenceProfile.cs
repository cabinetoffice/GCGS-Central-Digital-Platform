using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Model;
using TenantLookup = CO.CDP.OrganisationInformation.Persistence.TenantLookup;

namespace CO.CDP.Tenant.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Tenant, Model.Tenant>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<TenantLookup, Model.TenantLookup>();
        CreateMap<TenantLookup.PersonUser, UserDetails>();
        CreateMap<TenantLookup.Tenant, UserTenant>();
        CreateMap<TenantLookup.Organisation, UserOrganisation>()
            .ForMember(m => m.Uri, o => o.MapFrom(src => new Uri($"/organisations/{src.Id}")));

        CreateMap<RegisterTenant, OrganisationInformation.Persistence.Tenant>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.Organisations, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());
    }
}