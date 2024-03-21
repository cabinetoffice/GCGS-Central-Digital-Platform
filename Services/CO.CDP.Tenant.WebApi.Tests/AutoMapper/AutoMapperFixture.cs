using AutoMapper;
using CO.CDP.Tenant.WebApi.AutoMapper;

namespace CO.CDP.Tenant.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );
    public IMapper Mapper => Configuration.CreateMapper();
}