using AutoMapper;
using CO.CDP.DataSharing.WebApi.AutoMapper;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public IMapper Mapper => Configuration.CreateMapper();

    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
}