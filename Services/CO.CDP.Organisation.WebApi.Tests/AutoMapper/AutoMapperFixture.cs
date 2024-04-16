using AutoMapper;
using CO.CDP.Organisation.WebApi.AutoMapper;

namespace CO.CDP.Organisation.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}