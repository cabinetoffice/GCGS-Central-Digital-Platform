using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Organisation, Model.Organisation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<OrganisationIdentifier, OrganisationInformation.Persistence.Organisation.OrganisationIdentifier>();
        CreateMap<OrganisationInformation.Persistence.Organisation.OrganisationIdentifier, OrganisationIdentifier>();
        CreateMap<OrganisationAddress, OrganisationInformation.Persistence.Organisation.OrganisationAddress>();
        CreateMap<OrganisationInformation.Persistence.Organisation.OrganisationAddress, OrganisationAddress>();

        CreateMap<OrganisationContactPoint, OrganisationInformation.Persistence.Organisation.OrganisationContactPoint>();
        CreateMap<OrganisationInformation.Persistence.Organisation.OrganisationContactPoint, OrganisationContactPoint>();


        CreateMap<RegisterOrganisation, OrganisationInformation.Persistence.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Tenant, o => o.MapFrom((_, _, _, context) => context.Items["Tenant"]));
    }
}
