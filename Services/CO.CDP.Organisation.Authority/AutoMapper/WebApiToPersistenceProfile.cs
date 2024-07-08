using AutoMapper;
using TenantLookup = CO.CDP.OrganisationInformation.Persistence.TenantLookup;

namespace CO.CDP.Organisation.Authority.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<TenantLookup, OrganisationInformation.TenantLookup>();
        CreateMap<TenantLookup.PersonUser, OrganisationInformation.UserDetails>();
        CreateMap<TenantLookup.Tenant, OrganisationInformation.UserTenant>();
        CreateMap<TenantLookup.Organisation, OrganisationInformation.UserOrganisation>()
            .ForMember(m => m.Uri, o => o.MapFrom(src => new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{src.Id}")));
    }
}