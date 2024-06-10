using AutoMapper;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Tenant, Model.Tenant>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<OrganisationInformation.Persistence.Person, Model.UserDetails>()
            .ForMember(m => m.Name, o => o.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(m => m.Urn, o => o.MapFrom(m => m.UserUrn))
            .ForMember(m => m.Email, o => o.MapFrom(m => m.Email));

        CreateMap<OrganisationInformation.Persistence.Person, Model.TenantLookup>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Tenants, opt => opt.MapFrom(src => src.Tenants));

        CreateMap<OrganisationInformation.Persistence.Tenant, Model.UserTenant>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Organisations, opt => opt.MapFrom(src => src.Organisations));

        CreateMap<OrganisationInformation.Persistence.Organisation, Model.UserOrganisation>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
          .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
          .ForMember(dest => dest.Uri, opt => opt.MapFrom(src => new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{src.Guid}")))
          .ForMember(dest => dest.Scopes, opt => opt.MapFrom(src => src.Roles.Select(r => r.ToString()).ToList()));

        CreateMap<RegisterTenant, OrganisationInformation.Persistence.Tenant>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.Organisations, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());
    }
}