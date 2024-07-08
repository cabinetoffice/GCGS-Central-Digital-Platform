using AutoMapper;
using CO.CDP.Organisation.Authority.AutoMapper;

namespace CO.CDP.Organisation.Authority.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}