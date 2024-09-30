using AutoMapper;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Person, Model.Person>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<RegisterPerson, OrganisationInformation.Persistence.Person>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Tenants, o => o.Ignore())
            .ForMember(m => m.Organisations, o => o.Ignore())
            .ForMember(m => m.PersonOrganisations, o => o.Ignore())
            .ForMember(m => m.Scopes, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());
    }
}