using AutoMapper;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Person.Persistence.Person, Person.WebApi.Model.Person>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));        

        CreateMap<RegisterPerson, Person.Persistence.Person>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore());
    }
}
