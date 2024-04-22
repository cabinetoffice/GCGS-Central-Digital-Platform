using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.Organisation, Model.Organisation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<OrganisationIdentifier, Persistence.Organisation.OrganisationIdentifier>();
        CreateMap<Persistence.Organisation.OrganisationIdentifier, OrganisationIdentifier>();
        CreateMap<OrganisationAddress, Persistence.Organisation.OrganisationAddress>();
        CreateMap<Persistence.Organisation.OrganisationAddress, OrganisationAddress>();

        CreateMap<OrganisationContactPoint, Persistence.Organisation.OrganisationContactPoint>();
        CreateMap<Persistence.Organisation.OrganisationContactPoint, OrganisationContactPoint>();


        CreateMap<RegisterOrganisation, Persistence.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore());
    }
}
