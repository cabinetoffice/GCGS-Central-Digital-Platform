namespace CO.CDP.Organisation.Authority.Tests.AutoMapper;
public class WebApiToPersistenceProfileTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    [Fact]
    public void ConfigurationIsValid()
    {
        mapperFixture.Configuration.AssertConfigurationIsValid();
    }
}