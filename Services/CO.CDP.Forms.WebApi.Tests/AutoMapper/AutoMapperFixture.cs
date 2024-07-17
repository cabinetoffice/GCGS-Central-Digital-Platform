using AutoMapper;
using CO.CDP.Forms.WebApi.AutoMapper;

namespace CO.CDP.Forms.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}