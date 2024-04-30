using AutoMapper;
using CO.CDP.Person.WebApi.AutoMapper;

namespace CO.CDP.Person.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}