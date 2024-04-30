namespace CO.CDP.Person.WebApi.Tests.AutoMapper;
public class WebApiToPersistenceProfileTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    [Fact]
    public void ConfigurationIsValid()
    {
        mapperFixture.Configuration.AssertConfigurationIsValid();
    }
}