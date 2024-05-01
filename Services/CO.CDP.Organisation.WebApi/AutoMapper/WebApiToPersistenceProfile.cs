using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<CDP.Persistence.OrganisationInformation.Organisation, Model.Organisation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<OrganisationIdentifier, CDP.Persistence.OrganisationInformation.Organisation.OrganisationIdentifier>();
        CreateMap<CDP.Persistence.OrganisationInformation.Organisation.OrganisationIdentifier, OrganisationIdentifier>();
        CreateMap<OrganisationAddress, CDP.Persistence.OrganisationInformation.Organisation.OrganisationAddress>();
        CreateMap<CDP.Persistence.OrganisationInformation.Organisation.OrganisationAddress, OrganisationAddress>();

        CreateMap<OrganisationContactPoint, CDP.Persistence.OrganisationInformation.Organisation.OrganisationContactPoint>();
        CreateMap<CDP.Persistence.OrganisationInformation.Organisation.OrganisationContactPoint, OrganisationContactPoint>();


        CreateMap<RegisterOrganisation, CDP.Persistence.OrganisationInformation.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore());
    }
}
