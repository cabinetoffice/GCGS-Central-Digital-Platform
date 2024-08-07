using AutoMapper;
using CO.CDP.Tenant.WebApi.Model;
using TenantLookup = CO.CDP.OrganisationInformation.Persistence.TenantLookup;

namespace CO.CDP.Tenant.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Tenant, Model.Tenant>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<TenantLookup, OrganisationInformation.TenantLookup>();
        CreateMap<TenantLookup.PersonUser, OrganisationInformation.UserDetails>();
        CreateMap<TenantLookup.Tenant, OrganisationInformation.UserTenant>();
        CreateMap<TenantLookup.Organisation, OrganisationInformation.UserOrganisation>()
            .ForMember(m => m.Uri, o => o.MapFrom(src => new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{src.Id}")));

        CreateMap<RegisterTenant, OrganisationInformation.Persistence.Tenant>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.Organisations, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());
    }
}